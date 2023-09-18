using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Image : Entity
{
    public Image(string uniqueId, string name, Device device, string imageTopic, string encoding, string contentType) : base(uniqueId, name, device)
    {
        
        ImageTopic = imageTopic;
        ContentType = contentType;
        ImageEncoding = encoding;
        UrlTopic = null;
    }

    public Image(string uniqueId, string name, Device device, string urlTopic) : base(uniqueId, name, device)
    {

        UrlTopic = urlTopic;
    }

    [JsonPropertyName("image_encoding")]
    public string? ImageEncoding { get; }

    [JsonPropertyName("image_topic")]
    public string? ImageTopic { get; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; }

    [JsonPropertyName("image_ur;")]
    public string? UrlTopic { get; }


}
