using System.Text.Json.Serialization;

namespace ha_autodiscovery.net;

//https://www.home-assistant.io/integrations/mqtt#mqtt-discovery
internal class Entity
{
    public Entity(string name, string uniqueId, Device device)
    {
        Name = name;
        UniqueId = uniqueId;
        Device = device;
    }


    public List<Availability>? Availability { get; set; }

    /*
     *  all - payload_available must be received on all configured availability topics before the entity is marked as online
     *  any - payload_available must be received on at least one configured availability topic before the entity is marked as online
     *  latest - the last payload_available or payload_not_available received on any configured availability topic controls the availability
     */
    [JsonPropertyName("availability_mode")]
    public string AvailabilityMode { get; set; } = "latest";

    //https://www.home-assistant.io/docs/configuration/templating/
    [JsonPropertyName("availability_template")]
    public string? AvailabilityTemplate { get; set; }

    [JsonPropertyName("availability_topic")]
    public string? AvailabilityTopic { get; set; }

    public Device Device { get; set; }

    [JsonPropertyName("enabled_by_default")]
    public bool EnabledByDefault { get; set; } = true;

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = "utf-8";

    //https://developers.home-assistant.io/docs/core/entity#generic-properties
    [JsonPropertyName("entity_category")]
    public string EntityCategory { get; set; } = "None";

    //https://www.home-assistant.io/docs/configuration/templating/
    public string? Icon { get; set; }

    //https://www.home-assistant.io/docs/configuration/templating/
    [JsonPropertyName("json_attributes_template")]
    public string? JsonAttributesTemplate { get; set; }

    [JsonPropertyName("json_attributes_topic")]
    public string? JsonAttributesTopic { get; set; }


    public string? Name { get; set; }

    [JsonPropertyName("unique_id")]
    public string? UniqueId { get; set; }

    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }


    //[JsonPropertyName("state_topic")]
    //public string StateTopic { get; set; }

    //[JsonPropertyName("command_topic")]
    //public string CommandTopic { get; set; }


    //[JsonPropertyName("device_class")]
    //public string DeviceClass { get; set; }

    //[JsonPropertyName("payload_on")]
    //public string PayloadOn { get; set; }
    //[JsonPropertyName("payload_off")]
    //public string PayloadOff { get; set; }

}

