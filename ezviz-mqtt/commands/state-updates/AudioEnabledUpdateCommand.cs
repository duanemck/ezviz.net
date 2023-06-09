using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.config;

namespace ezviz_mqtt.commands.state_updates
{
    internal class AudioEnabledUpdateCommand : BaseStateUpdateCommand
    {
        public AudioEnabledUpdateCommand(IEzvizClient client, JsonOptions jsonOptions, IStatePublishCommand updateCommand) : base(client, jsonOptions, updateCommand)
        {
        }

        protected override Task UpdateStateCustom(Camera camera, string newState)
        {
            return camera.ToggleAudio(newState == jsonOptions.SerializeTrueAs);
        }
    }
}
