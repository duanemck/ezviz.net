namespace ezviz_mqtt
{
    internal interface IMqttHandler
    {
        Task ConnectToMqtt(params string[] topicsToSubscribe);

        void SendRawMqtt(string topic, object? data);

        void SendMqtt(string topic, object? data, bool retain = false, bool jsonSerialize = true);

        Task EnsureConnected();

        void Subscribe(string topic);

        bool IsConnected { get; }

        void Shutdown();

        event Action<string,string> MessageReceived;
    }
}