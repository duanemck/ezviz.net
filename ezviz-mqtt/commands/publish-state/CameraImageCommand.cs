using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class CameraImageCommand : BasePublishStateCommand
    {
        public CameraImageCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            var lastAlarm = camera.GetLastAlarm().Result;
            if (!string.IsNullOrEmpty(lastAlarm?.LastAlarmPicture))
            {
                var imageBase64 = ezvizClient.GetAlarmImageBase64(lastAlarm.LastAlarmPicture).Result;
                mqttHandler.SendRawMqtt(topics.GetTopic(Topics.Image, camera), imageBase64);
            }
        }
    }
}
