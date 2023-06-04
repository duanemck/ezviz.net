using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Sensor : Entity
{
    public Sensor(string uniqueId, string name, Device device, string stateTopic) : base(uniqueId, name, device)
    {
        StateTopic = stateTopic;
        EntityCategory = null;
    }
    
    [JsonPropertyName("state_topic")]
    public string StateTopic { get; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; set; }

    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; set; }

}
