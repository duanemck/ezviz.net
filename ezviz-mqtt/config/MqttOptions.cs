namespace ezviz_mqtt.config;
internal class MqttOptions
{
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public IDictionary<string, string> Topics { get; set; } = null!;
}

