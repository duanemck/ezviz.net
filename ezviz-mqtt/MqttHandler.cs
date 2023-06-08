using Microsoft.Extensions.Logging;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Text.Json;
using ezviz_mqtt.config;
using Microsoft.Extensions.Options;
using ezviz_mqtt.health;

namespace ezviz_mqtt
{
    internal class MqttHandler : IMqttHandler
    {
        private readonly MqttClient mqttClient;
        private readonly ILogger<MqttHandler> logger;
        private readonly MqttServiceState serviceState;
        private MqttOptions mqttConfig;
        private JsonSerializerOptions jsonSerializationOptions;

        private ICollection<Action<string,string>> eventHandlers = new List<Action<string,string>>();

        public bool IsConnected => mqttClient?.IsConnected ?? false;

        public MqttHandler(
            ILogger<MqttHandler> logger, 
            IOptions<MqttOptions> mqttOptions, 
            IOptions<JsonOptions> jsonOptions,
            MqttServiceState serviceState
            )
        {
            this.logger = logger;
            this.serviceState = serviceState;
            mqttConfig = mqttOptions.Value;
            mqttClient = new MqttClient(mqttConfig.Host);
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

        event Action<string, string> IMqttHandler.MessageReceived
        {
            add
            {
                eventHandlers.Add(value);
            }

            remove
            {
                eventHandlers.Remove(value);
            }
        }

        public async Task ConnectToMqtt(params string[] topicsToSubscribe) 
        {
            logger.LogInformation("Connecting to MQTT {0}", mqttConfig.Host);
            mqttClient.MqttMsgPublishReceived += MessageReceived;

            mqttClient.Connect(mqttConfig.Client, mqttConfig.Username, mqttConfig.Password, false, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, true, mqttConfig.ServiceLwtTopic, mqttConfig.ServiceLwtOfflineMessage, false, 60);
            SendLwtForService(mqttConfig.ServiceLwtOnlineMessage);

            serviceState.MqttConnected = true;

            foreach (var topic in topicsToSubscribe)
            {
                Subscribe(topic);
            }
        }

        public async Task EnsureConnected()
        {
            int counter = 0;
            while (!mqttClient.IsConnected && counter < mqttConfig.ConnectRetries)
            {
                try
                {
                    logger.LogWarning("MQTT connection seems to be down, reconnecting");
                    await ConnectToMqtt();
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
        }

        private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
            logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");

            foreach (var handler in eventHandlers)
            {
                handler(e.Topic, utfString);
            }
        }

        private void SendLwtForService(string message)
        {
            var topic = mqttConfig.ServiceLwtTopic;
            SendMqtt(topic, message, true, false);
        }


        public void SendRawMqtt(string topic, object? data)
        {
            SendMqtt(topic, data, false, false);
        }

        public void SendMqtt(string topic, object? data, bool retain = false, bool jsonSerialize = true)
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
                logger.LogDebug($"Publishing [{message}] to [{topic}]");
                
                mqttClient.Publish(topic, Encoding.UTF8.GetBytes(dataString), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, retain);                
            }
        }

        public void Shutdown()
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }

        public void Subscribe(string topic)
        {
            logger.LogInformation("Subscribing to {0}", topic);
            mqttClient.Subscribe(new[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });            
        }
    }
}
