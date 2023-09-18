using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class LastAlarmImageUrlCommand : BasePublishStateCommand
    {
        public LastAlarmImageUrlCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
             mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.LastAlarmImageUrl, camera), (camera?.GetLastAlarm().Result)?.LastAlarmPicture);
        }
    }
}
