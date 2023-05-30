
using System.Text.Json.Serialization;

namespace ha_autodiscovery.net;
internal class Availability
{
    public Availability(string topic, string online, string offline) : this(topic)
    {
        PayloadAvailable = online;
        PayloadNotAvailable = offline;
    }

    public Availability(string topic)
    {
        Topic = topic;
    }

    public string Topic { get; set; }

    [JsonPropertyName("payload_available")]
    public string PayloadAvailable { get; set; } = "online";

    [JsonPropertyName("payload_not_available")]
    public string PayloadNotAvailable { get; set; } = "offline";

    //https://www.home-assistant.io/docs/configuration/templating/
    [JsonPropertyName("value_template")]
    public string? ValueTemplate { get; set; }
}

