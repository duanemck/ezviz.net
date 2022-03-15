using ezviz.net;
using ezviz.net.domain;
using ezviz.net.exceptions;
using ezviz_mqtt.config;
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
    private readonly EzvizOptions ezvizConfig;
    private readonly MqttOptions mqttConfig;
    private readonly JsonOptions jsonConfig;
    private readonly PollingOptions pollingConfig;

    private readonly EzvizClient ezvizClient;
    private readonly MqttClient mqttClient;
    private readonly JsonSerializerOptions jsonSerializationOptions;

    private DateTime LastFullPoll = default(DateTime);
    private DateTime LastAlarmPoll = default(DateTime);

    private IEnumerable<Camera> cameras = new List<Camera>();

    private IDictionary<string, List<string>> TrackedAlarms = new Dictionary<string, List<string>>();

    public MqttPublisher(
        ILogger<MqttPublisher> logger,
        IOptions<EzvizOptions> ezvizOptions,
        IOptions<MqttOptions> mqttOptions,
        IOptions<JsonOptions> jsonOptions,
        IOptions<PollingOptions> pollingOptions)
    {
        this.logger = logger;
        ezvizConfig = ezvizOptions.Value;
        mqttConfig = mqttOptions.Value;
        jsonConfig = jsonOptions.Value;
        ezvizClient = new EzvizClient(ezvizConfig.Username, ezvizConfig.Password);
        mqttClient = new MqttClient(mqttConfig.Host);
        pollingConfig = pollingOptions.Value;

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
            ConnectToMqtt();
            if (string.IsNullOrEmpty(ezvizConfig?.Username) || string.IsNullOrEmpty(ezvizConfig?.Username))
            {
                throw new EzvizNetException("Please provide an ezviz username and password");
            }
            logger.LogInformation("Logging in to ezviz API as {0}", ezvizConfig.Username);
            await ezvizClient.Login();
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

    private void SendRetainedMqtt(string topicKey, string? serial, object data, bool jsonSerialize = true)
    {
        SendMqtt(topicKey, serial, data, true, jsonSerialize);
    }

    private void SendMqtt(string topicKey, string? serial, object data, bool retain = false, bool jsonSerialize = true)
    {
        if (serial == null)
        {
            throw new ArgumentNullException(nameof(serial));
        }
        var topic = mqttConfig.Topics[topicKey].Replace("{serial}", serial);
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
        var timeSinceLastFullPoll = DateTime.Now - LastFullPoll;
        var timeSinceLastAlarmPoll = DateTime.Now - LastAlarmPoll;
        try
        {

            if (timeSinceLastFullPoll.TotalMinutes >= pollingConfig.Cameras)
            {
                await PollCameras(stoppingToken);
                LastFullPoll = DateTime.Now;
            }

            if (timeSinceLastAlarmPoll.TotalMinutes >= pollingConfig.Alarms)
            {
                await PollAlarms(stoppingToken);
                LastAlarmPoll = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to poll ezviz API");
            LastFullPoll = LastAlarmPoll = DateTime.Now;
        }
    }

    private async Task PollCameras(CancellationToken stoppingToken)
    {
        logger.LogInformation("Polling ezviz API for full camera details");
        cameras = await ezvizClient.GetCameras(stoppingToken);

        foreach (var camera in cameras)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            SendMqtt("status", camera.SerialNumber, camera);
            SendRetainedMqtt("lwt", camera.SerialNumber, (camera.Online ?? false) ? "ON" : "OFF", false);
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
                            SendMqtt("alarm", camera.SerialNumber, alarm);
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
        if (string.IsNullOrEmpty(mqttConfig.Username))
        {
            mqttClient.Connect("ezviz.net");
        }
        else
        {
            mqttClient.Connect("ezviz.net", mqttConfig.Username, mqttConfig.Password);
        }

        var commandTopic = mqttConfig.Topics["command"].Replace("{serial}", "#");
        logger.LogInformation("Subscribing to {0}", commandTopic);

        mqttClient.Subscribe(new[] { commandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
        logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");
    }

    public void Dispose()
    {
        if (mqttClient != null && mqttClient.IsConnected)
        {
            mqttClient.Disconnect();
        }
    }
}

