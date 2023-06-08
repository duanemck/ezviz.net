using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.util;
using uPLibrary.Networking.M2Mqtt;

namespace ezviz_mqtt.commands.state_updates
{
    internal abstract class BaseStateUpdateCommand : IStateUpdateCommand
    {
        protected readonly IEzvizClient client;
        protected readonly TopicExtensions topics;
        protected readonly IMqttHandler mqttHandler;

        public BaseStateUpdateCommand(IEzvizClient client, TopicExtensions topics, IMqttHandler mqttHandler)
        {
            this.client = client;
            this.topics = topics;
            this.mqttHandler = mqttHandler;
        }

        public abstract Task UpdateState(Camera camera, string newState);
    }
}
