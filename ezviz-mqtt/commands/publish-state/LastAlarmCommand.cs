using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class LastAlarmCommand : BasePublishStateCommand
    {
        public LastAlarmCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            //TODO: More detail in attributes
             mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.LastAlarm, camera), (camera?.GetLastAlarm().Result)?.ToString());
        }
    }
}
