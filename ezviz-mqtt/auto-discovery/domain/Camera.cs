using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Camera : Entity
{
    public Camera(string uniqueId, string name, Device device, string topic) : base(uniqueId, name, device)
    {
        Topic = topic;
    }

    //None - Raw binary
    //b64 - Base64
    [JsonPropertyName("image_encoding")]
    public string? ImageEncoding { get; set; } = null;

    public string Topic { get; set; }

}

