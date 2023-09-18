using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class LastAlarmImageCommand : BasePublishStateCommand
    {
        public LastAlarmImageCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            var lastAlarm = camera.GetLastAlarm().Result;
            if (!string.IsNullOrEmpty(lastAlarm?.LastAlarmPicture))
            {
                var imageBase64 = ezvizClient.GetAlarmImageBase64(lastAlarm.LastAlarmPicture).Result;
                mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.LastAlarmImage, camera), imageBase64);
            }
        }
    }
}
