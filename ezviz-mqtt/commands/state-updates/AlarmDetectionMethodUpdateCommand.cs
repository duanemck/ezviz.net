using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.config;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.state_updates
{
    internal class AlarmDetectionMethodUpdateCommand : BaseStateUpdateCommand
    {
        public AlarmDetectionMethodUpdateCommand(IEzvizClient client, JsonOptions jsonOptions, IStatePublishCommand updateCommand) : base(client, jsonOptions, updateCommand)
        {
        }

        protected override Task UpdateStateCustom(Camera camera, string newState)
        {
            return camera.SetAlarmDetectionMethod(EnumX.Parse<AlarmDetectionMethod>(newState));            
        }
    }
}
