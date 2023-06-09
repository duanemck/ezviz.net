using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.config;

namespace ezviz_mqtt.commands.state_updates
{
    internal class ArmDisarmCommand : BaseStateUpdateCommand
    {
        public ArmDisarmCommand(IEzvizClient client, JsonOptions jsonOptions, IStatePublishCommand updateCommand) : base(client, jsonOptions, updateCommand)
        {
        }

        protected override async Task UpdateStateCustom(Camera camera, string newState)
        {
            if (newState == jsonOptions.SerializeTrueAs)
            {
                await camera.Arm();
            }
            else
            {
                await camera.Disarm();
            }
        }
    }
}
