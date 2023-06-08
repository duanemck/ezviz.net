using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class DetectionSensitivityCommand : BasePublishStateCommand
    {
        public DetectionSensitivityCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler) : base(topics, booleanConverter, mqttHandler)
        {
        }

        public override void Publish(Camera camera)
        {
            mqttHandler.SendRawMqtt(topics.GetStatusTopic<DetectionSensitivityLevel>(camera), (camera?.DetectionSensitivity ?? DetectionSensitivityLevel.Unknown).ToString());
        }
    }
}
