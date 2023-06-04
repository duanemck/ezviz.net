using ezviz.net;
using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz.net.exceptions;
using ezviz.net.util;
using ezviz_mqtt.auto_discovery;
using ezviz_mqtt.auto_discovery.domain;
using ezviz_mqtt.config;
using ezviz_mqtt.health;
using ezviz_mqtt.util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Camera = ezviz.net.domain.Camera;


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
    private readonly IPushNotificationLogger pushLogger;
    private readonly MqttClient mqttClient;
    private readonly JsonSerializerOptions jsonSerializationOptions;

    private DateTime LastFullPoll = default(DateTime);
    private DateTime LastAlarmPoll = default(DateTime);

    private IEnumerable<Camera> cameras = new List<Camera>();

    private IDictionary<string, List<string>> TrackedAlarms = new Dictionary<string, List<string>>();

    private readonly string _globalCommandTopic;
    private readonly string _globalStateTopic;
    private readonly string _deviceCommandTopic;
    private readonly string _deviceStatusTopic;


    private bool discoveryMessagesSent = false;

    private TopicExtensions mqttTopics;

    public MqttPublisher(
        ILogger<MqttPublisher> logger,
        IOptions<EzvizOptions> ezvizOptions,
        IOptions<MqttOptions> mqttOptions,
        IOptions<JsonOptions> jsonOptions,
        IOptions<PollingOptions> pollingOptions,
        MqttServiceState serviceState,
        IEzvizClient ezvizClient,
        IPushNotificationLogger pushLogger)
    {
        this.logger = logger;
        this.serviceState = serviceState;
        ezvizConfig = ezvizOptions.Value;
        mqttConfig = mqttOptions.Value;
        jsonConfig = jsonOptions.Value;
        this.ezvizClient = ezvizClient;
        this.pushLogger = pushLogger;
        mqttClient = new MqttClient(mqttConfig.Host);
        pollingConfig = pollingOptions.Value;

        mqttTopics = new TopicExtensions(mqttConfig.Topics, StringComparer.OrdinalIgnoreCase);

        _globalCommandTopic = mqttTopics.GetTopic(Topics.GlobalCommand);
        _deviceCommandTopic = mqttTopics.GetTopic(Topics.Command, "+");
        _globalStateTopic = mqttTopics.GetTopic(Topics.GlobalStatus);
        _deviceStatusTopic = $"{mqttTopics.GetTopic(Topics.Status, "+").Replace("{entity}", "+")}/set";

        jsonSerializationOptions = new JsonSerializerOptions()
        {
            //WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
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

            if (ezvizConfig.EnablePushNotifications)
            {
                await ezvizClient.EnablePushNotifications(pushLogger, HandlePushedAlarmMessage);
            }

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




    private async Task SendMqttForCamera(Camera camera)
    {
        var topic = mqttTopics.GetTopic(Topics.Stat, camera.SerialNumber);
        SendMqtt(topic, camera);

        SendRawMqtt(mqttTopics.GetStatusTopic<AlarmSound>(camera), (camera?.AlarmSoundLevel ?? AlarmSound.Unknown).ToString());
        SendRawMqtt(mqttTopics.GetStatusTopic<AlarmDetectionMethod>(camera), (camera?.AlarmDetectionMethod ?? AlarmDetectionMethod.Unknown).ToString());
        SendRawMqtt(mqttTopics.GetStatusTopic<DetectionSensitivityLevel>(camera), (camera?.DetectionSensitivity ?? DetectionSensitivityLevel.Unknown).ToString());

        var booleanConverter = new BooleanConvertor(jsonConfig);
        SendRawMqtt(mqttTopics.GetStatusTopic("upgrade_available", camera), booleanConverter.SerializeBoolean(camera.UpgradeAvailable));
        SendRawMqtt(mqttTopics.GetStatusTopic("upgrade_in_progress", camera), booleanConverter.SerializeBoolean(camera.UpgradeInProgress));
        SendRawMqtt(mqttTopics.GetStatusTopic("upgrade_percent", camera), camera.UpgradePercent);
        SendRawMqtt(mqttTopics.GetStatusTopic("rtsp_encrypted", camera), booleanConverter.SerializeBoolean(camera.IsEncrypted));
        SendRawMqtt(mqttTopics.GetStatusTopic("battery_level", camera), camera?.BatteryLevel);
        SendRawMqtt(mqttTopics.GetStatusTopic("pir_status", camera), camera.PirStatus);
        SendRawMqtt(mqttTopics.GetStatusTopic("disk_capacity", camera), camera.DiskCapacityGB);

        //More detail in attributes
        SendRawMqtt(mqttTopics.GetStatusTopic("last_alarm", camera), (await camera?.GetLastAlarm())?.ToString());

        SendRawMqtt(mqttTopics.GetStatusTopic("alarm_schedule_enabled", camera), booleanConverter.SerializeBoolean(camera.AlarmScheduleEnabled));
        SendRawMqtt(mqttTopics.GetStatusTopic("sleeping", camera), booleanConverter.SerializeBoolean(camera.Sleeping));
        SendRawMqtt(mqttTopics.GetStatusTopic("audio_enabled", camera), booleanConverter.SerializeBoolean(camera.AudioEnabled));
        SendRawMqtt(mqttTopics.GetStatusTopic("infrared_enabled", camera), booleanConverter.SerializeBoolean(camera.InfraredEnabled));
        SendRawMqtt(mqttTopics.GetStatusTopic("status_led_enabled", camera), booleanConverter.SerializeBoolean(camera.StateLedEnabled));
        SendRawMqtt(mqttTopics.GetStatusTopic("motion_tracking_enabled", camera), booleanConverter.SerializeBoolean(camera.MobileTrackingEnabled));
        SendRawMqtt(mqttTopics.GetStatusTopic("notify_when_offline", camera), booleanConverter.SerializeBoolean(camera.NotifyOffline));
        SendRawMqtt(mqttTopics.GetStatusTopic("armed", camera), booleanConverter.SerializeBoolean(camera.Armed));
        SendRawMqtt(mqttTopics.GetStatusTopic("trigger_alarm", camera), booleanConverter.SerializeBoolean(false));
    }

    private void SendMqttForAlarm(Camera camera, Alarm alarm)
    {
        var topic = mqttTopics.GetTopic(Topics.Alarm, camera.SerialNumber);
        SendMqtt(topic, alarm);
    }

    private void SendLwtForService(string message)
    {
        var topic = mqttConfig.ServiceLwtTopic;
        SendMqtt(topic, message, true, false);
    }



    private void SendLwtForCamera(string? serial, string message)
    {
        if (serial == null)
        {
            throw new ArgumentNullException(nameof(serial));
        }

        SendMqtt(mqttTopics.GetLwtTopicForCamera(serial), message, true, false);
    }

    private void SendRawMqtt(string topic, object? data)
    {
        SendMqtt(topic, data, false, false);
    }

    private void SendMqtt(string topic, object? data, bool retain = false, bool jsonSerialize = true)
    {
        if (data == null)
        {
            return;
        }
#pragma warning disable IL2026
        var dataString = jsonSerialize ? JsonSerializer.Serialize(data, jsonSerializationOptions) : data.ToString();
#pragma warning restore IL2026
        if (dataString != null)
        {
            var message = dataString.Substring(0, dataString.Length < 200 ? dataString.Length : 200);
            if (message.Length < dataString.Length)
            {
                message += "...";
            }
            logger.LogInformation($"Publishing [{message}] to [{topic}]");
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
        if (ezvizConfig.EnablePushNotifications)
        {
            await ezvizClient.CheckPushConnection();
        }
        logger.LogInformation("Checking Defence Mode");
        var defenceMode = await ezvizClient.GetDefenceMode();
        SendMqtt(_globalStateTopic.Replace("{command}", MQTT_COMMAND_DEFENCEMODE), defenceMode.ToString(), false, false);

        logger.LogInformation("Polling ezviz API for full camera details");
        cameras = await ezvizClient.GetCameras(stoppingToken);

        if (!discoveryMessagesSent)
        {
            var manager = new AutoDiscoveryManager(logger, mqttClient, mqttTopics, mqttConfig);
            foreach (var camera in cameras)
            {
                manager.AutoDiscoverCamera(camera);
            }
            discoveryMessagesSent = true;
        }

        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            await SendMqttForCamera(camera);
            SendLwtForCamera(camera.SerialNumber, (camera.Online ?? false) ? mqttConfig.ServiceLwtOnlineMessage : mqttConfig.ServiceLwtOfflineMessage);
        }
        logger.LogInformation("Polling done, published details of {0} cameras", cameras.Count());
    }

    private List<string> GetTrackedAlarmsForCamera(string serialNumber)
    {
        if (!TrackedAlarms.ContainsKey(serialNumber))
        {
            TrackedAlarms[serialNumber] = new List<string>();
        }
        return TrackedAlarms[serialNumber];
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

        logger.LogInformation("Subscribing to {0}", _deviceStatusTopic);
        mqttClient.Subscribe(new[] { _deviceStatusTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        

        serviceState.MqttConnected = true;

    }

    private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
        logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");

        if (e.Topic.StartsWith(mqttTopics.GetTopic(Topics.GlobalCommand).Replace("/#", "")))
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
    private const string MQTT_COMMAND_ARMED = "armed";
    private const string MQTT_COMMAND_SET_STATE = "set";

    private async Task HandleStateSet(string serial, string stateEntity, string newValue)
    {
        logger.LogInformation($"Setting [{stateEntity}] for [{serial}] to [{newValue}]");
        //Update device and then echo new state back

        var device = cameras.FirstOrDefault(c => c.SerialNumber == serial);
        if (device == null)
        {
            logger.LogError($"Could not update state, unknonw device [{serial}]");
            return;
        }
        switch (stateEntity)
        {
            case "armed":
                if (newValue == "ON")
                {
                    await device.Arm();
                }else
                {
                    await device.Disarm();
                }
                SendRawMqtt(mqttTopics.GetStatusTopic("armed", device), newValue);
                break;
        }

    }

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
        try
        {
            Shutdown().Wait();
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }
        catch { }
    }

    public async Task Shutdown()
    {
        if (ezvizConfig.EnablePushNotifications && ezvizClient != null)
        {
            await ezvizClient.Shutdown();
        }
    }
}

