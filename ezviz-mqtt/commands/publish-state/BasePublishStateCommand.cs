using ezviz.net;
using ezviz.net.domain;
using ezviz_mqtt.commands.state_updates;
using ezviz_mqtt.util;
using uPLibrary.Networking.M2Mqtt;

namespace ezviz_mqtt.commands.publish_state
{
    internal abstract class BasePublishStateCommand : IStatePublishCommand
    {
        protected readonly TopicExtensions topics;
        protected readonly BooleanConvertor booleanConverter;
        protected readonly IMqttHandler mqttHandler;

        public BasePublishStateCommand(TopicExtensions topics, BooleanConvertor booleanConvertor, IMqttHandler mqttHandler)
        {
            this.topics = topics;
            this.booleanConverter = booleanConvertor;
            this.mqttHandler = mqttHandler;
        }

        public abstract void Publish(Camera camera);
    }
}
