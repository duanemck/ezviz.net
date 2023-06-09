using ezviz.net;
using ezviz.net.domain;
using ezviz.net.domain.deviceInfo;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.config;
using ezviz_mqtt.util;

namespace ezviz_mqtt.commands.state_updates
{
    internal class AlarmSoundUpdateCommand : BaseStateUpdateCommand
    {
        public AlarmSoundUpdateCommand(IEzvizClient client, JsonOptions jsonOptions, IStatePublishCommand updateCommand) : base(client, jsonOptions, updateCommand)
        {
        }

        protected override Task UpdateStateCustom(Camera camera, string newState)
        {
            return camera.SetAlarmSoundLevel(EnumX.Parse<AlarmSound>(newState), true);
        }
    }
}
