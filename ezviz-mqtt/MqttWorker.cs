using ezviz.net;
using ezviz.net.domain;
using ezviz.net.exceptions;
using ezviz.net.util;
using ezviz_mqtt.auto_discovery;
using ezviz_mqtt.commands;
using ezviz_mqtt.config;
using ezviz_mqtt.health;
using ezviz_mqtt.util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ezviz_mqtt;

/*
 * 
 * TODO:
 * - Figure out how to expose RTSP stream in Auto Discover
 * - Publish extra attributes
 *      - IP addresses
 *      - Last Alarm
 *  - Send LastAlarmImage as Base64
 *  Open RTSP Stream to send static image when polling
 *  
 *  
 *  - Auto discover service level (Away mode, etc)
 *  - MQTT command to fetch static image
 */


internal class MqttWorker : IMqttWorker
{
    private readonly ILogger<MqttWorker> logger;
    private readonly MqttServiceState serviceState;
    private readonly EzvizOptions ezvizConfig;
    private readonly MqttOptions mqttConfig;
    private readonly PollingOptions pollingConfig;

    private readonly IEzvizClient ezvizClient;
    private readonly IPushNotificationLogger pushLogger;
    private readonly IStateCommandFactory commandFactory;
    private readonly IMqttHandler mqttHandler;
    private readonly IAutoDiscoveryManager autoDiscoverManager;
    private DateTime LastFullPoll = default;
    private DateTime LastAlarmPoll = default;
    private DateTime LastAutoDiscoverMessage = default;

    private IEnumerable<Camera> cameras = new List<Camera>();

    private readonly IDictionary<string, List<string>> trackedAlarms = new Dictionary<string, List<string>>();

    private readonly string _globalCommandTopic;
    private readonly string _globalStateTopic;
    private readonly string _deviceCommandTopic;
    private readonly string _deviceStatusTopic;


    private readonly TopicExtensions mqttTopics;

    public MqttWorker(
        ILogger<MqttWorker> logger,
        IOptions<EzvizOptions> ezvizOptions,
        IOptions<MqttOptions> mqttOptions,
        IOptions<PollingOptions> pollingOptions,
        MqttServiceState serviceState,
        IEzvizClient ezvizClient,
        IPushNotificationLogger pushLogger,
        IStateCommandFactory commandFactory,
        IMqttHandler mqttHandler,
        IAutoDiscoveryManager autoDiscoverManager)
    {
        this.logger = logger;
        this.serviceState = serviceState;
        ezvizConfig = ezvizOptions.Value;
        mqttConfig = mqttOptions.Value;
        this.ezvizClient = ezvizClient;
        this.pushLogger = pushLogger;
        this.commandFactory = commandFactory;
        this.mqttHandler = mqttHandler;
        this.autoDiscoverManager = autoDiscoverManager;
        pollingConfig = pollingOptions.Value;

        mqttHandler.MessageReceived += MqttHandler_MessageReceived;

        mqttTopics = new TopicExtensions(mqttConfig.Topics, StringComparer.OrdinalIgnoreCase);
        _globalCommandTopic = mqttTopics.GetTopic(Topics.GlobalCommand);
        _globalStateTopic = mqttTopics.GetTopic(Topics.GlobalStatus);

        _deviceCommandTopic = mqttTopics.GetTopic(Topics.Command, "+");
        _deviceStatusTopic = $"{mqttTopics.GetTopic(Topics.Status, "+").Replace("{entity}", "+")}/set";
    }

    private void MqttHandler_MessageReceived(string topic, string message)
    {

        if (topic.StartsWith(mqttTopics.GetTopic(Topics.GlobalCommand).Replace("/#", "")))
        {
            Task.WaitAll(HandleGlobalCommand(topic, message));
        }
        else
        {
            Task.WaitAll(HandleCameraCommand(topic, message));
        }
    }

    private int InitRetries = 0;

    public async Task Init()
    {
        try
        {
            ezvizClient.LogAllResponses = pollingConfig.LogAllResponses;

            mqttHandler.ConnectToMqtt(_deviceCommandTopic, _globalCommandTopic, _deviceStatusTopic);
            if (string.IsNullOrEmpty(ezvizConfig?.Username) || string.IsNullOrEmpty(ezvizConfig?.Password))
            {
                throw new EzvizNetException("Please provide an ezviz username and password");
            }
            logger.LogInformation("Logging in to ezviz API as {0}", ezvizConfig.Username);
            var user = await ezvizClient.Login(ezvizConfig.Username, ezvizConfig.Password);

            if (ezvizConfig.EnablePushNotifications)
            {
                await ezvizClient.EnablePushNotifications(pushLogger, HandlePushedAlarmMessage);
            }
            autoDiscoverManager.AutoDiscoverServiceEntities(user);

        }
        catch (Exception ex)
        {
            if (InitRetries < mqttConfig.ConnectRetries)
            {
                logger.LogError(ex, $"Could not initialize MQTT publisher");
                InitRetries++;
                await Task.Delay(mqttConfig.ConnectRetryDelaySeconds * 1000);
                await Init();
            }
            else
            {
                throw new EzvizNetException("Max retries exceeded, giving up");
            }
        }
    }

    private void HandlePushedAlarmMessage(Alarm alarm)
    {
        var camera = cameras.FirstOrDefault(camera => camera.SerialNumber == alarm.DeviceSerial);
        if (camera == null)
        {
            throw new EzvizNetException($"Received alarm for an untracked camera [{alarm.DeviceSerial}]");
        }
        HandleNewAlarms(camera, alarm).Wait();
    }

    private void SendMqttForCamera(Camera camera)
    {
        var topic = mqttTopics.GetTopic(Topics.Stat, camera.SerialNumber);
        mqttHandler.SendMqtt(topic, camera);

        commandFactory.GetAllStatePublishCommands().ToList().ForEach(cmd => cmd.Publish(camera));
    }

    private void SendMqttForAlarm(Camera camera, Alarm alarm)
    {
        var topic = mqttTopics.GetTopic(Topics.Alarm, camera.SerialNumber);
        mqttHandler.SendMqtt(topic, alarm);
    }

    private void SendLwtForCamera(string? serial, string message)
    {
        if (serial == null)
        {
            throw new ArgumentNullException(nameof(serial));
        }

        mqttHandler.SendMqtt(mqttTopics.GetLwtTopicForCamera(serial), message, true, false);
    }

    public async Task PublishAsync(CancellationToken stoppingToken, bool force = false)
    {
        serviceState.MqttConnected = mqttHandler.IsConnected;
        await mqttHandler.EnsureConnected();

        var timeSinceLastFullPoll = DateTime.Now - LastFullPoll;
        var timeSinceLastAutoDiscover = DateTime.Now - LastAutoDiscoverMessage;
        var timeSinceLastAlarmPoll = DateTime.Now - LastAlarmPoll;
        try
        {
            if (force || (timeSinceLastFullPoll.TotalMinutes >= pollingConfig.Cameras))
            {
                await PollCameras(stoppingToken, timeSinceLastAutoDiscover.TotalHours >= 1);
                LastFullPoll = DateTime.Now;
                LastAutoDiscoverMessage = DateTime.Now;
                serviceState.LastStatusCheck = LastFullPoll;
            }
            if (force || (timeSinceLastAlarmPoll.TotalMinutes >= pollingConfig.Alarms))
            {
                await PollAlarms(stoppingToken);
                LastAlarmPoll = DateTime.Now;
                serviceState.LastAlarmCheck = LastAlarmPoll;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to poll ezviz API");
            LastAutoDiscoverMessage = DateTime.Now;
            LastFullPoll = LastAlarmPoll = DateTime.Now;
            serviceState.MostRecentError = ex.Message;
            serviceState.MostRecentErrorTime = DateTime.Now;
        }
    }

    private async Task PollCameras(CancellationToken stoppingToken, bool sendAutoDisover)
    {
        if (ezvizConfig.EnablePushNotifications)
        {
            await ezvizClient.CheckPushConnection();
        }
        logger.LogInformation("Checking Defence Mode");
        var defenceMode = await ezvizClient.GetDefenceMode();
        mqttHandler.SendMqtt(_globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE), defenceMode.ToString(), false, false);

        logger.LogInformation("Polling ezviz API for full camera details");
        cameras = await ezvizClient.GetCameras(stoppingToken);

        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            if (sendAutoDisover)
            {
                autoDiscoverManager.AutoDiscoverCamera(camera);
                //Let HA process the messages before sendinf the actual values
                await Task.Delay(10 * 10000);
            }

            SendMqttForCamera(camera);
            SendLwtForCamera(camera.SerialNumber, (camera.Online ?? false) ? mqttConfig.ServiceLwtOnlineMessage : mqttConfig.ServiceLwtOfflineMessage);
        }
        logger.LogInformation("Polling done, published details of {0} cameras", cameras.Count());
    }

    private List<string> GetTrackedAlarmsForCamera(string serialNumber)
    {
        if (!trackedAlarms.ContainsKey(serialNumber))
        {
            trackedAlarms[serialNumber] = new List<string>();
        }
        return trackedAlarms[serialNumber];
    }

    private Task HandleNewAlarms(Camera camera, Alarm alarm)
    {
        return HandleNewAlarms(camera, new List<Alarm> { alarm }, false);
    }

    private async Task HandleNewAlarms(Camera camera, IEnumerable<Alarm> alarms, bool cleanupAlarms = true)
    {
        var cameraTrackedAlarms = GetTrackedAlarmsForCamera(camera.SerialNumber);
        foreach (var alarm in alarms.Where(a => !cameraTrackedAlarms.Contains(a.AlarmId)))
        {
            if (alarm.IsEarlierThan(pollingConfig.AlarmStaleAgeMinutes))
            {
                logger.LogDebug($"Alarm [{alarm.AlarmId}] is newer than {pollingConfig.AlarmStaleAgeMinutes} minute(s)");
                if (!cameraTrackedAlarms.Contains(alarm.AlarmId))
                {
                    logger.LogInformation($"New alarm found [{alarm.AlarmId}]");
                    if (alarm.DownloadedPicture == null)
                    {
                        alarm.DownloadedPicture = await ezvizClient.GetAlarmImageBase64(alarm);
                    }
                    SendMqttForAlarm(camera, alarm);
                }
            }
            else
            {
                logger.LogDebug($"Alarm [{alarm.AlarmId}] is older than {pollingConfig.AlarmStaleAgeMinutes} minute(s)");
            }
            cameraTrackedAlarms.Add(alarm.AlarmId);
            logger.LogDebug($"Alarm [{alarm.AlarmId}] is now being tracked and won't be processed again");
        }
        if (cleanupAlarms)
        {
            var alarmsToStopTracking = new List<string>();
            foreach (var trackedAlarm in cameraTrackedAlarms)
            {
                if (!alarms.Any(a => a.AlarmId == trackedAlarm))
                {
                    logger.LogDebug($"Removing alarm [{trackedAlarm}] from tracking as it's no longer being reported by the API");
                    alarmsToStopTracking.Add(trackedAlarm);
                }
            }
            alarmsToStopTracking.ForEach(alarmId => cameraTrackedAlarms.Remove(alarmId));
        }
    }

    private async Task PollAlarms(CancellationToken stoppingToken)
    {
        logger.LogInformation("Polling ezviz API for full recent alarms");
        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                logger.LogInformation($"Checking [{camera.Name}] for alarms");
                var alarms = (await camera.GetAlarms()).Where(a => a.IsCheck == 0);
                logger.LogDebug($"Found {alarms.Count()} unread alarms");

                await HandleNewAlarms(camera, alarms);
            }
            catch
            {
                //Do nothing here but let loop continue in case a camera fails 
            }

        }
        logger.LogInformation("Polling alarms done");
    }

    private async Task HandleCameraCommand(string topic, string message)
    {
        string command = topic.Substring(topic.LastIndexOf("/") + 1);
        string topicWithoutCommand = topic.Replace($"/{command}", "");
        string serial = topicWithoutCommand.Substring(topicWithoutCommand.LastIndexOf("/") + 1);

        switch (command)
        {
            case MQTT_COMMAND_SET_STATE:
                string entity = serial;
                topicWithoutCommand = topicWithoutCommand.Replace($"/{entity}", "");
                serial = topicWithoutCommand.Substring(topicWithoutCommand.LastIndexOf("/") + 1);
                await HandleStateSet(serial, entity, message);
                break;
            default:
                logger.LogInformation($"Unknown MQTT command received {command}");
                break;
        }
    }

    private const string MQTT_COMMAND_DEFENCEMODE = "defenceMode";
    private const string MQTT_COMMAND_POLL_NOW = "poll";
    private const string MQTT_COMMAND_SET_STATE = "set";

    private async Task HandleStateSet(string serial, string stateEntity, string newValue)
    {
        logger.LogInformation($"Setting [{stateEntity}] for [{serial}] to [{newValue}]");

        var device = cameras.FirstOrDefault(c => c.SerialNumber == serial);
        if (device == null)
        {
            logger.LogError($"Could not update state, unknonw device [{serial}]");
            return;
        }

        await commandFactory.GetStateUpdateCommand(stateEntity).UpdateState(device, newValue);
    }

    private async Task HandleGlobalCommand(string topic, string message)
    {
        string command = topic.Substring(topic.LastIndexOf("/") + 1);
        switch (command)
        {
            case MQTT_COMMAND_DEFENCEMODE:
                var defenceMode = EnumX.Parse<DefenceMode>(message);
                await ezvizClient.SetDefenceMode(defenceMode);
                mqttHandler.SendMqtt(_globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE), defenceMode.ToString(), false, false);
                break;
            case MQTT_COMMAND_POLL_NOW:
                await PublishAsync(default, true);
                break;
            default:
                logger.LogInformation($"Unknown MQTT command received {command}");
                break;
        }
    }

    public void Dispose()
    {
        try
        {
            Shutdown().Wait();

        }
        catch { }
    }

    public async Task Shutdown()
    {
        if (ezvizConfig.EnablePushNotifications && ezvizClient != null)
        {
            await ezvizClient.Shutdown();
        }
        mqttHandler.Shutdown();
    }
}

