using ezviz.net;
using ezviz.net.domain;
using ezviz.net.exceptions;
using ezviz_systemd.net.config;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ezviz_systemd.net
{
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

        public async Task Init()
        {
            ConnectToMqtt();
            if (string.IsNullOrEmpty(ezvizConfig?.Username) || string.IsNullOrEmpty(ezvizConfig?.Username))
            {
                throw new EzvizNetException("Please provide an ezviz username and password");
            }
            logger.LogInformation("Logging in to ezviz API as {0}", ezvizConfig.Username);
            await ezvizClient.Login();
        }

        private void SendMqtt(string topicKey, string? serial, object data, bool jsonSerialize = true)
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
                mqttClient.Publish(topic, Encoding.UTF8.GetBytes(dataString));
            }
        }


        public async Task PublishAsync(CancellationToken stoppingToken)
        {
            try
            {
                var timeSinceLastFullPoll = DateTime.Now - LastFullPoll;
                if (timeSinceLastFullPoll.TotalMinutes >= pollingConfig.Cameras)
                {
                    await PollCameras(stoppingToken);
                    LastFullPoll = DateTime.Now;
                }
                var timeSinceLastAlarmPoll = DateTime.Now - LastAlarmPoll;
                if (timeSinceLastAlarmPoll.TotalMinutes >= pollingConfig.Alarms)
                {
                    await PollAlarms(stoppingToken);
                    LastAlarmPoll = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to poll ezviz API");
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
                await camera.GetDetectionSensibilityAsync();
                //logger.LogInformation(camera.Name);
                //logger.LogInformation("- ON:");
                //foreach (var sw in camera.Switches.Where(sw=>sw.Enable))
                //{
                //    logger.LogInformation($"--- {sw.Type}");
                //}
                //logger.LogInformation("- OFF:");
                //foreach (var sw in camera.Switches.Where(sw => !sw.Enable))
                //{
                //    logger.LogInformation($"--- {sw.Type}");
                //}
                SendMqtt("status", camera.SerialNumber, camera);
                SendMqtt("lwt", camera.SerialNumber, (camera.Online ?? false) ? "ON" : "OFF", false);
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
                logger.LogInformation($"Checking [{camera.Name}] for alarms");

                var alarms = (await camera.GetAlarms()).Where(a => (DateTime.Now - a.AlarmStartTimeParsed).TotalSeconds <= 300);
                if (alarms.Any())
                {
                    SendMqtt("alarm", camera.SerialNumber, alarms);
                }

               
            }
            logger.LogInformation("Polling alarms done");
        }

        void ConnectToMqtt()
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

        void MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
            logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");
        }



        public void Dispose()
        {
            if (mqttClient != null)
            {
                mqttClient.Disconnect();
            }
        }
    }
}
