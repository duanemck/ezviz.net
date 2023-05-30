using System.Text.Json.Serialization;

namespace ha_autodiscovery.net;

public class Camera : Entity
{
    public Camera(string name, string uniqueId, Device device, string topic) : base(name, uniqueId, device)
    {
        Topic = topic;
    }

    //None - Raw binary
    //b64 - Base64
    [JsonPropertyName("image_encoding")]
    public string ImageEncoding { get; set; } = "None";

    public string Topic { get; set; }

}

