
using System.Text.Json.Serialization;

namespace ezviz_mqtt.home_assistant;
internal class Availability
{
    public Availability(string topic, string online, string offline)
    {
        Topic = topic;
        PayloadAvailable = online;
        PayloadNotAvailable = offline;
    }

    public string Topic { get; set; }

    [JsonPropertyName("payload_available")]
    public string PayloadAvailable { get; set; }

    [JsonPropertyName("payload_not_available")]
    public string PayloadNotAvailable { get; set; }
}

