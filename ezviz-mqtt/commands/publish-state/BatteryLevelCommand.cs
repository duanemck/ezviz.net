using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class BatteryLevelCommand : BasePublishStateCommand
    {
        public BatteryLevelCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            if (camera.BatteryLevel is not null)
            {
                mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.BatteryLevel, camera), camera?.BatteryLevel);
            }
        }
    }
}
