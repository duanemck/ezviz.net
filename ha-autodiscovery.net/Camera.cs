using System.Text.Json.Serialization;

namespace ha_autodiscovery.net;

internal class Camera : Entity
{
    public Camera(string topic) : base("", "", null)
    {
        Topic = topic;
    }

    //None - Raw binary
    //b64 - Base64
    [JsonPropertyName("image_encoding")]
    public string ImageEncoding { get; set; } = "None";

    public string Topic { get; set; }

}

