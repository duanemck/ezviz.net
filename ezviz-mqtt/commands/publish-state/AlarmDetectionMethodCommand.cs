using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class AlarmDetectionMethodCommand : BasePublishStateCommand
    {
        public AlarmDetectionMethodCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            mqttHandler.SendRawMqtt(topics.GetStatusTopic<AlarmDetectionMethod>(camera), (camera?.AlarmDetectionMethod ?? AlarmDetectionMethod.Unknown).ToString());
        }
    }
}
