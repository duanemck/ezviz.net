using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.config;
using ezviz_mqtt.util;
using uPLibrary.Networking.M2Mqtt;

namespace ezviz_mqtt.commands.state_updates
{
    internal abstract class BaseStateUpdateCommand : IStateUpdateCommand
    {
        protected readonly IEzvizClient client;
        protected readonly JsonOptions jsonOptions;
        protected readonly IStatePublishCommand updateCommand;

        public BaseStateUpdateCommand(IEzvizClient client, JsonOptions jsonOptions, IStatePublishCommand updateCommand)
        {
            this.client = client;
            this.jsonOptions = jsonOptions;
            this.updateCommand = updateCommand;
        }

        public async Task UpdateState(Camera camera, string newState)
        {
            await UpdateStateCustom(camera, newState);
            updateCommand.Publish(camera);
        }

        protected abstract Task UpdateStateCustom(Camera camera, string newState);
    }
}
