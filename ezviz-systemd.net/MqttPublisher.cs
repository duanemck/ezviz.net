using ezviz.net;
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

        private readonly EzvizClient ezvizClient;
        private readonly MqttClient mqttClient;
        private readonly JsonSerializerOptions jsonSerializationOptions;

        public MqttPublisher(ILogger<MqttPublisher> logger, IOptions<EzvizOptions> ezvizOptions, IOptions<MqttOptions> mqttOptions, IOptions<JsonOptions> jsonOptions)
        {
            this.logger = logger;
            ezvizConfig = ezvizOptions.Value;
            mqttConfig = mqttOptions.Value;
            jsonConfig = jsonOptions.Value;
            ezvizClient = new EzvizClient(ezvizConfig.Username, ezvizConfig.Password);
            mqttClient = new MqttClient(mqttConfig.Host);


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
            logger.LogInformation("Logging in to ezviz API");
            await ezvizClient.Login();
        }

        private void SendMqtt(string topicKey, string serial, object data, bool jsonSerialize = true)
        {
            var topic = mqttConfig.Topics[topicKey].Replace("{serial}", serial);
            var dataString = jsonSerialize ? JsonSerializer.Serialize(data, jsonSerializationOptions) : data.ToString();
            mqttClient.Publish(topic, Encoding.UTF8.GetBytes(dataString));
        }

        public async Task PublishAsync()
        {
            try
            {
                logger.LogInformation("Polling ezviz API");
                
                var cameras = await ezvizClient.GetCameras();
                foreach (var camera in cameras)
                {
                    SendMqtt("status", camera.SerialNumber, camera);
                    SendMqtt("lwt", camera.SerialNumber, (camera.Online ?? false) ? "ON" : "OFF", false);
                    
                    var alarms = (await camera.GetAlarms()).Where(a => (DateTime.Now - a.AlarmStartTimeParsed).TotalSeconds <= 300);
                    if (alarms.Any())
                    {
                        SendMqtt("alarm", camera.SerialNumber, alarms);                        
                    }
                }
                logger.LogInformation("Polling done, published details of {0} cameras", cameras.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to poll ezviz API");
            }
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
    }
}
