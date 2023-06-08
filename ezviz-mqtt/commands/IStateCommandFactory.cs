using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.commands.state_updates;

namespace ezviz_mqtt.commands
{
    internal interface IStateCommandFactory
    {
        public IStateUpdateCommand GetStateUpdateCommand(string command);
        public IStatePublishCommand GetStatePublishCommand(string command);
        public IEnumerable<IStatePublishCommand> GetAllStatePublishCommands();
    }
}