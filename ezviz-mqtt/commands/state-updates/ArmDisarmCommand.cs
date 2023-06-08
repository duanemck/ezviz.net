using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.state_updates
{
    internal class ArmDisarmCommand : BaseStateUpdateCommand
    {
        public ArmDisarmCommand(IEzvizClient client, TopicExtensions topics, IMqttHandler mqttHandler) : base(client, topics, mqttHandler)
        {
        }

        public override async Task UpdateState(Camera camera, string newState)
        {
            if (newState == "ON")
            {
                await camera.Arm();
            }
            else
            {
                await camera.Disarm();
            }
            mqttHandler.SendRawMqtt(topics.GetStatusTopic(StateEntities.Armed, camera), newState);
        }
    }
}
