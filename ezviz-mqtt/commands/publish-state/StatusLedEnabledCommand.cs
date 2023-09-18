using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class StatusLedEnabledCommand : BasePublishStateCommand
    {
        public StatusLedEnabledCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.StatusLedEnabled, camera), booleanConverter.SerializeBoolean(camera.StateLedEnabled));
        }
    }
}
