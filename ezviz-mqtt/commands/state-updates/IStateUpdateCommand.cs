using ezviz.net.domain;

namespace ezviz_mqtt.commands.state_updates
{
    internal interface IStateUpdateCommand
    {
        Task UpdateState(Camera camera, string newState);
    }
}
