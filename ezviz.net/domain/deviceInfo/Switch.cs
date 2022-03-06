using System.Text.Json.Serialization;

namespace ezviz.net.domain.deviceInfo;

public class Switch
{
    public string DeviceSerial { get; set; }
    public int ChannelNo { get; set; }
    public SwitchType Type { get; set; }
    [JsonPropertyName("Enabled")]
    public bool Enable { get; set; }
}

