using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class NotifyWhenOfflineCommand : BasePublishStateCommand
    {
        public NotifyWhenOfflineCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.NotifyWhenOffline, camera), booleanConverter.SerializeBoolean(camera.NotifyOffline));
        }
    }
}
