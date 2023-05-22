using ezviz.net;
using ezviz.net.domain;
using ezviz.net.exceptions;
using ezviz_mqtt.config;
using ezviz_mqtt.health;
using ezviz_mqtt.home_assistant;
using ezviz_mqtt.util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ezviz_mqtt;

internal class MqttPublisher : IMqttPublisher
{
    private readonly ILogger<MqttPublisher> logger;
    private readonly MqttServiceState serviceState;
    private readonly EzvizOptions ezvizConfig;
    private readonly MqttOptions mqttConfig;
    private readonly JsonOptions jsonConfig;
    private readonly PollingOptions pollingConfig;

    private readonly IEzvizClient ezvizClient;
    private readonly MqttClient mqttClient;
    private readonly JsonSerializerOptions jsonSerializationOptions;

    private DateTime LastFullPoll = default(DateTime);
    private DateTime LastAlarmPoll = default(DateTime);

    private IEnumerable<Camera> cameras = new List<Camera>();

    private IDictionary<string, List<string>> TrackedAlarms = new Dictionary<string, List<string>>();

    private readonly string _globalCommandTopic;
    private readonly string _globalStateTopic;
    private readonly string _deviceCommandTopic;

    private const string homeAssistantDiscoveryTopic = "homeassistant/{device_class}/{unique_id}/config";
    private string homeAssistantUniqueId;
    private bool discoveryMessagesSent = false;

    public MqttPublisher(
        ILogger<MqttPublisher> logger,
        IOptions<EzvizOptions> ezvizOptions,
        IOptions<MqttOptions> mqttOptions,
        IOptions<JsonOptions> jsonOptions,
        IOptions<PollingOptions> pollingOptions,
        MqttServiceState serviceState,
        IEzvizClient ezvizClient)
    {
        this.logger = logger;
        this.serviceState = serviceState;
        ezvizConfig = ezvizOptions.Value;
        mqttConfig = mqttOptions.Value;
        jsonConfig = jsonOptions.Value;
        //ezvizClient = new EzvizClient(ezvizConfig.Username, ezvizConfig.Password);.
        this.ezvizClient = ezvizClient;
        mqttClient = new MqttClient(mqttConfig.Host);
        pollingConfig = pollingOptions.Value;
        _globalCommandTopic = mqttConfig.Topics["globalCommand"];
        _deviceCommandTopic = mqttConfig.Topics["command"].Replace("{serial}", "+");
        _globalStateTopic = mqttConfig.Topics["globalStatus"];

        jsonSerializationOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters =
                {
                    new BooleanConvertor(jsonOptions.Value)
                }
        };
    }

    private int InitRetries = 0;

    public async Task Init()
    {
        try
        {
            ezvizClient.LogAllResponses = pollingConfig.LogAllResponses;

            ConnectToMqtt();
            if (string.IsNullOrEmpty(ezvizConfig?.Username) || string.IsNullOrEmpty(ezvizConfig?.Password))
            {
                throw new EzvizNetException("Please provide an ezviz username and password");
            }
            logger.LogInformation("Logging in to ezviz API as {0}", ezvizConfig.Username);
            var user = await ezvizClient.Login(ezvizConfig.Username, ezvizConfig.Password);
            homeAssistantUniqueId = $"ezviz_bridge_{user.UserId}";
        }
        catch (Exception ex)
        {
            logger.LogError($"Could not initialize MQTT publisher [{ex.Message}]");
            if (InitRetries < mqttConfig.ConnectRetries)
            {
                logger.LogError($"Could not initialize MQTT publisher [{ex.Message}]");
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

    private void SendHomeAssistantDiscoveryMessages(IEnumerable<Camera> cameras)
    {
        //TODO: Think about this some more and then complete it
        return;
        //var device = new Device(homeAssistantUniqueId, "EZVIZ Mqtt Bridge", "duanemck", "");
        //var availability = new Availability(mqttConfig.ServiceLwtTopic, mqttConfig.ServiceLwtOnlineMessage, mqttConfig.ServiceLwtOfflineMessage);
        //var defenceModeEntity = new Entity()
        //{
        //    Name = "EZVIZ Defence Mode",
        //    UniqueId = $"{homeAssistantUniqueId}_defence_mode",
        //    Device = device,
        //    Availability = availability,
        //    CommandTopic = _globalCommandTopic.Replace("#", MQTT_COMMAND_DEFENCEMODE),
        //    StateTopic = _globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE),
        //    DeviceClass = "switch",
        //    PayloadOn = "Away",
        //    PayloadOff = "Home"
        //};

        //SendMqtt(homeAssistantDiscoveryTopic, defenceModeEntity, true);

        //foreach (var camera in cameras)
        //{
        //    var cameraAvailability = new Availability(LwtTopicForCamera(camera.SerialNumber), mqttConfig.ServiceLwtOnlineMessage, mqttConfig.ServiceLwtOfflineMessage);

        //    var entity = new Entity()
        //    {
        //        Name = camera.Name,
        //        UniqueId = $"{homeAssistantUniqueId}_{camera.SerialNumber}",
        //        Device = device,
        //        Availability = cameraAvailability,
        //        CommandTopic = _globalCommandTopic,
        //        StateTopic = _globalStateTopic,
        //        DeviceClass = "switch",
        //        PayloadOn = "Away",
        //        PayloadOff = "Home"
        //    };
        //}
    }


    private void SendMqttForCamera(string topicKey, string? serial, object data, bool retain = false)
    {
        if (serial == null)
        {
            throw new ArgumentNullException(nameof(serial));
        }
        var topic = mqttConfig.Topics[topicKey].Replace("{serial}", serial);
        SendMqtt(topic, data, retain);
    }

    private void SendLwtForService(string message)
    {
        var topic = mqttConfig.ServiceLwtTopic;
        SendMqtt(topic, message, true, false);
    }

    private string LwtTopicForCamera(string? serial)
    {
        return mqttConfig.Topics["lwt"].Replace("{serial}", serial);
    }

    private void SendLwtForCamera(string? serial, string message)
    {
        if (serial == null)
        {
            throw new ArgumentNullException(nameof(serial));
        }
        
        SendMqtt(LwtTopicForCamera(serial), message, true, false);
    }

    private void SendMqtt(string topic, object data, bool retain = false, bool jsonSerialize = true)
    {
#pragma warning disable IL2026
        var dataString = jsonSerialize ? JsonSerializer.Serialize(data, jsonSerializationOptions) : data.ToString();
#pragma warning restore IL2026
        if (dataString != null)
        {
            var message = dataString.Substring(0, dataString.Length < 50 ? dataString.Length : 50);
            if (message.Length < dataString.Length)
            {
                message += "...";
            }
            logger.LogDebug($"Publishing [{message}] to [{topic}]");
            mqttClient.Publish(topic, Encoding.UTF8.GetBytes(dataString), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, retain);
        }
    }

    public async Task PublishAsync(CancellationToken stoppingToken)
    {
        serviceState.MqttConnected = mqttClient.IsConnected;
        int counter = 0;
        while (!mqttClient.IsConnected && counter < mqttConfig.ConnectRetries)
        {
            try
            {
                logger.LogWarning("MQTT connection seems to be down, reconnecting");
                ConnectToMqtt();
            }
            catch (Exception e)
            {
                logger.LogError("Could not connect to MQTT broker", e);
                await Task.Delay(mqttConfig.ConnectRetryDelaySeconds * 1000);
            }
            finally
            {
                counter++;
            }
        }

        var timeSinceLastFullPoll = DateTime.Now - LastFullPoll;
        var timeSinceLastAlarmPoll = DateTime.Now - LastAlarmPoll;
        try
        {
            if (timeSinceLastFullPoll.TotalMinutes >= pollingConfig.Cameras)
            {
                await PollCameras(stoppingToken);
                LastFullPoll = DateTime.Now;
                serviceState.LastStatusCheck = LastFullPoll;
            }
            if (timeSinceLastAlarmPoll.TotalMinutes >= pollingConfig.Alarms)
            {
                await PollAlarms(stoppingToken);
                LastAlarmPoll = DateTime.Now;
                serviceState.LastAlarmCheck = LastAlarmPoll;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to poll ezviz API");
            LastFullPoll = LastAlarmPoll = DateTime.Now;
            serviceState.MostRecentError = ex.Message;
            serviceState.MostRecentErrorTime = DateTime.Now;
        }
    }

    private async Task PollCameras(CancellationToken stoppingToken)
    {
        logger.LogInformation("Checking Defence Mode");
        var defenceMode = await ezvizClient.GetDefenceMode();
        SendMqtt(_globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE), defenceMode.ToString(), false, false);

        logger.LogInformation("Polling ezviz API for full camera details");
        cameras = await ezvizClient.GetCameras(stoppingToken);

        if (!discoveryMessagesSent)
        {
            SendHomeAssistantDiscoveryMessages(cameras);
            discoveryMessagesSent = true;
        }

        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            SendMqttForCamera("status", camera.SerialNumber, camera);
            SendLwtForCamera(camera.SerialNumber, (camera.Online ?? false) ? mqttConfig.ServiceLwtOnlineMessage : mqttConfig.ServiceLwtOfflineMessage);
        }
        logger.LogInformation("Polling done, published details of {0} cameras", cameras.Count());
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
            if (!TrackedAlarms.ContainsKey(camera.SerialNumber))
            {
                TrackedAlarms[camera.SerialNumber] = new List<string>();
            }
            var cameraTrackedAlarms = TrackedAlarms[camera.SerialNumber];
            try
            {
                logger.LogInformation($"Checking [{camera.Name}] for alarms");
                var alarms = (await camera.GetAlarms()).Where(a => a.IsCheck == 0);
                logger.LogDebug($"Found {alarms.Count()} unread alarms");

                foreach (var alarm in alarms.Where(a => !cameraTrackedAlarms.Contains(a.AlarmId)))
                {
                    if (alarm.IsEarlierThan(pollingConfig.AlarmStaleAgeMinutes))
                    {
                        logger.LogDebug($"Alarm [{alarm.AlarmId}] is newer than {pollingConfig.AlarmStaleAgeMinutes} minute(s)");
                        if (!cameraTrackedAlarms.Contains(alarm.AlarmId))
                        {
                            logger.LogInformation($"New alarm found [{alarm.AlarmId}]");
                            alarm.DownloadedPicture = await ezvizClient.GetAlarmImageBase64(alarm);
                            SendMqttForCamera("alarm", camera.SerialNumber, alarm);
                        }
                    }
                    else
                    {
                        logger.LogDebug($"Alarm [{alarm.AlarmId}] is older than {pollingConfig.AlarmStaleAgeMinutes} minute(s)");
                    }
                    cameraTrackedAlarms.Add(alarm.AlarmId);
                    logger.LogDebug($"Alarm [{alarm.AlarmId}] is now being tracked and won't be processed again");
                }
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
            catch
            {
                //Do nothing here but let loop continue in case a camera fails 
            }

        }
        logger.LogInformation("Polling alarms done");
    }

    private void ConnectToMqtt()
    {
        logger.LogInformation("Connecting to MQTT {0}", mqttConfig.Host);
        mqttClient.MqttMsgPublishReceived += MessageReceived;

        mqttClient.Connect(mqttConfig.Client, mqttConfig.Username, mqttConfig.Password, false, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true, mqttConfig.ServiceLwtTopic, mqttConfig.ServiceLwtOfflineMessage, false, 60);
        SendLwtForService(mqttConfig.ServiceLwtOnlineMessage);

        logger.LogInformation("Subscribing to {0}", _deviceCommandTopic);
        mqttClient.Subscribe(new[] { _deviceCommandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        logger.LogInformation("Subscribing to {0}", _globalCommandTopic);
        mqttClient.Subscribe(new[] { _globalCommandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        serviceState.MqttConnected = true;

    }

    private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
        logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");

        if (e.Topic.StartsWith(mqttConfig.Topics["globalCommand"].Replace("/#", "")))
        {
            Task.WaitAll(HandleGlobalCommand(e.Topic, utfString));
        }
        else
        {
            Task.WaitAll(HandleCameraCommand(e.Topic, utfString));
        }
    }

    private async Task HandleCameraCommand(string topic, string message)
    {
        string command = topic.Substring(topic.LastIndexOf("/") + 1);
        string topicWithoutCommand = topic.Replace($"/{command}", "");
        string serial = topicWithoutCommand.Substring(topicWithoutCommand.LastIndexOf("/") + 1);

        //TODO: Clean this up
        switch (command)
        {
            case MQTT_COMMAND_ARMED:
                var camera = cameras.FirstOrDefault(c => c.SerialNumber == serial);
                if (camera != null)
                {
                    if (message == "ON")
                    {
                        await camera.Arm();
                    }
                    else if (message == "OFF")
                    {
                        await camera.Disarm();
                    }
                }
                break;
            default:
                logger.LogInformation($"Unknown MQTT command received {command}");
                break;
        }
    }

    private const string MQTT_COMMAND_DEFENCEMODE = "defenceMode";
    private const string MQTT_COMMAND_ARMED = "armed";

    private async Task HandleGlobalCommand(string topic, string message)
    {
        string command = topic.Substring(topic.LastIndexOf("/") + 1);
        switch (command)
        {
            case MQTT_COMMAND_DEFENCEMODE:
                var defenceMode = EnumX.Parse<DefenceMode>(message);
                await ezvizClient.SetDefenceMode(defenceMode);
                SendMqtt(_globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE), defenceMode.ToString(), false, false);
                break;
            default:
                logger.LogInformation($"Unknown MQTT command received {command}");
                break;
        }
    }

    public void Dispose()
    {
        if (mqttClient != null && mqttClient.IsConnected)
        {
            mqttClient.Disconnect();
        }
    }
}

