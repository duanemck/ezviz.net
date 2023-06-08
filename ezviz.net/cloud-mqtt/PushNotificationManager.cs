using ezviz.net;
using ezviz.net.cloud_mqtt;
using ezviz.net.domain;
using ezviz.net.exceptions;
using ezviz.net.util;
using Refit;
using System.Text;
using System.Text.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ezviz_mqtt.cloud_mqtt
{
    internal class PushNotificationManager
    {
        private string? mqttClientId = null;
        private string? mqttTicket = null;
        private readonly IEzvizClient client;
        private readonly IPushNotificationLogger logger;
        private readonly Action<Alarm> messageHandler;
        private IEzvizPushApi pushApi = null!;
        private MqttClient mqttClient = null!;
        private EzvizUser user = null!;
        private LoginSession session = null!;
        private SystemConfigInfo systemConfig = null!;
        private bool shuttingDown = false;

        public PushNotificationManager(IEzvizClient client, IPushNotificationLogger logger, Action<Alarm> messageHandler)
        {
            this.client = client;
            this.logger = logger;
            this.messageHandler = messageHandler;
        }

        public async Task Connect(EzvizUser user, LoginSession session, SystemConfigInfo systemConfig)
        {

            this.user = user;
            this.session = session;
            this.systemConfig = systemConfig;
            await OpenPushNotificationStream();
        }

        public async Task OpenPushNotificationStream()
        {
            if (mqttClient == null || mqttClient.IsConnected)
            {
                return;
            }
            await RegisterForEzvizPush();
            await StartEzvizPush();

            mqttClient = new MqttClient(systemConfig.PushAddr, 1882, false, MqttSslProtocols.None, null, null);
            logger.LogInformation("Connecting to Ezviz Cloud MQTT {0}", systemConfig.PushAddr);
            mqttClient.MqttMsgPublishReceived += MessageReceived;
            mqttClient.MqttMsgSubscribed += NewSubscription;
            mqttClient.ConnectionClosed += MqttClosed;

            mqttClient.Connect(mqttClientId, Constants.MQTT_APP_KEY, Constants.APP_SECRET, false, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false, "lwt", "online", true, 60);

            var topic = $"{Constants.MQTT_APP_KEY}/ticket/{mqttTicket}";
            logger.LogInformation("Subscribing to [{0}]", topic);
            mqttClient.Subscribe(new[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        }

        public async Task Shutdown()
        {
            logger.LogInformation("Shutting down push notification subscription");
            shuttingDown = true;
            if (mqttClient != null && mqttClient.IsConnected)
            {
                await StopEzvizPush();
                mqttClient.Disconnect();
            }
        }

        private void MqttClosed(object sender, EventArgs e)
        {
            logger.LogInformation("MQTT Connection Closed");
            if (!shuttingDown)
            {
                try
                {
                    OpenPushNotificationStream().Wait();
                }
                catch
                {
                }
            }
        }

        private void NewSubscription(object sender, MqttMsgSubscribedEventArgs e)
        {
            logger.LogInformation("Subscribed [{0}]", e.MessageId);
        }

        private async Task RegisterForEzvizPush()
        {
            GetApi(systemConfig.PushAddr);
            var payload = new Dictionary<string, object>()
            {
                { "appKey", Constants.MQTT_APP_KEY },
                { "clientType", "5" },
                { "mac", Constants.FEATURE_CODE },
                { "token", "123456" },
                { "version", "v1.3.0"}
            };
            var authToken = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Constants.MQTT_APP_KEY}:{Constants.APP_SECRET}"))}";

            var response = await pushApi.Register(payload, authToken);
            if (response.Status == 200)
            {
                mqttClientId = response?.Data?.ClientId;
            }
            else
            {
                throw new EzvizNetException($"Could not register for push notifications [{response.Message}]");
            }
        }

        private async Task StartEzvizPush()
        {
            if (mqttClientId == null)
            {
                throw new EzvizNetException("Not able to start Push, no client id");
            }
            var payload = new Dictionary<string, object>()
            {
                { "appKey", Constants.MQTT_APP_KEY },
                { "clientId", mqttClientId },
                { "clientType", "5" },
                { "sessionId", session.SessionId },
                { "username", user.Username},
                { "token", "123456" },
            };
            var response = await pushApi.StartPush(payload);

            if (response.Status == 200)
            {
                mqttTicket = response?.Ticket;
            }
            else
            {
                throw new EzvizNetException($"Could not register for push notifications [{response.Message}]");
            }
        }

        private async Task StopEzvizPush()
        {
            if (mqttClientId == null)
            {
                throw new EzvizNetException("Not able to start Push, no client id");
            }
            var payload = new Dictionary<string, object>()
            {
                { "appKey", Constants.MQTT_APP_KEY },
                { "clientId", mqttClientId },
                { "clientType", "5" },
                { "sessionId", session.SessionId },
                { "username", user.Username}
            };
            await pushApi.StopPush(payload);
        }

        private IEzvizPushApi GetApi(string baseUrl)
        {
            if (pushApi == null)
            {
                pushApi = RestService.For<IEzvizPushApi>($"https://{baseUrl}");
            }
            return pushApi;
        }


        private readonly JsonSerializerOptions deserializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private void MessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string utfString = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length);
                logger.LogInformation($"Received message via MQTT from [{e.Topic}] => {utfString}");
#pragma warning disable IL2026 
                var message = JsonSerializer.Deserialize<PushMessage>(utfString, deserializationOptions);
#pragma warning restore IL2026
                if (message != null)
                {
                    var alarm = new Alarm(message);
                    alarm.DownloadedPicture = client.GetAlarmImageBase64(alarm).Result;
                    messageHandler(alarm);
                }
            }
            catch
            {
                logger.LogInformation("Could not parse push message");
            }
        }
    }
}
