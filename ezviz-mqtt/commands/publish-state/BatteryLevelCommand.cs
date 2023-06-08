using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class BatteryLevelCommand : BasePublishStateCommand
    {
        public BatteryLevelCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler) : base(topics, booleanConverter, mqttHandler)
        {
        }

        public override void Publish(Camera camera)
        {
             mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.BatteryLevel, camera), camera?.BatteryLevel);
        }
    }
}
