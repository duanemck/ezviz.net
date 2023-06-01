using System.Text.Json.Serialization;

namespace ha_autodiscovery.net;

public class Device
{
    public Device(string uniqueId, string name, string manufacturer, string model)
    {
        Identifiers.Add(uniqueId);
        Name = name;
        Manufacturer = manufacturer;
        Model = model;
    }


    [JsonPropertyName("configuration_url")]
    public string? ConfigurationUrl { get; set; }

    public IEnumerable<string[]> Connections { get; set; } = new List<string[]>();

    [JsonPropertyName("hw_version")]
    public string? HardwareVersion { get; set; }

    public ICollection<string> Identifiers { get; set; } = new List<string>();

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("suggested_area")]
    public string? SuggestedArea { get; set; }


    [JsonPropertyName("sw_version")]
    public string? SoftwareVersion { get; set; }


    [JsonPropertyName("via_device")]
    public string? ViaDevice { get; set; }
}

