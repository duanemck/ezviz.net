using ezviz.net.domain;

namespace ezviz_mqtt.commands.publish_state
{
    internal interface IStatePublishCommand
    {
        void Publish(Camera camera);
    }
}
