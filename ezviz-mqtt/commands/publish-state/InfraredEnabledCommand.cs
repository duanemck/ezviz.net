using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class InfraredEnabledCommand : BasePublishStateCommand
    {
        public InfraredEnabledCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
             mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.InfraredEnabled, camera), booleanConverter.SerializeBoolean(camera.InfraredEnabled));
        }
    }
}
