using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class Cloud
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string ResourceId { get; set; }
    public string DeviceSerial { get; set; }
    public int ChannelNo { get; set; }
    public int Status { get; set; }
    public int TotalDays { get; set; }
    public int ValidDays { get; set; }
}

