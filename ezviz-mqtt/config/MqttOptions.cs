namespace ezviz_mqtt.config;
internal class MqttOptions
{
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public int ConnectRetries { get; set; }

    public int ConnectRetryDelaySeconds { get; set; }

    public IDictionary<string, string> Topics { get; set; } = null!;
}

