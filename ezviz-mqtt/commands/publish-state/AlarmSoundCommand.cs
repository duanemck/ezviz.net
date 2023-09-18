using ezviz.net;
using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.publish_state
{
    internal class AlarmSoundCommand : BasePublishStateCommand
    {
        public AlarmSoundCommand(TopicExtensions topics, BooleanConvertor booleanConverter, IMqttHandler mqttHandler, IEzvizClient client) : base(topics, booleanConverter, mqttHandler, client)
        {
        }

        public override void Publish(Camera camera)
        {
            mqttHandler.SendRawMqtt(topics.GetStatusTopic<AlarmSound>(camera), (camera?.AlarmSoundLevel ?? AlarmSound.Unknown).ToString());
        }
    }
}
