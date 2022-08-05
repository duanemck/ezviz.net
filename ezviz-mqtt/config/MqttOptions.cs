namespace ezviz_mqtt.config;
internal class MqttOptions
{
    public string Client { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public int ConnectRetries { get; set; }

    public int ConnectRetryDelaySeconds { get; set; }

    public string ServiceLwtTopic { get; set; } = null!;
    public string ServiceLwtOfflineMessage { get; set; } = null!;
    public string ServiceLwtOnlineMessage { get; set; } = null!;

    public IDictionary<string, string> Topics { get; set; } = null!;
}

