using System.Text.Json.Serialization;

namespace ezviz.net.domain.deviceInfo;

public class EzvizDeviceInfo
{
    public string Name { get; set; } = null!;
    public string DeviceSerial { get; set; } = null!;
    public string FullSerial { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public string DevicePicPrefix { get; set; } = null!;
    public string Version { get; set; } = null!;
    [JsonIgnore]
    public string SupportExt { get; set; } = null!;
    public int Status { get; set; }
    public string UserDeviceCreateTime { get; set; } = null!;
    public int ChannelNumber { get; set; }
    public bool Hik { get; set; }
    public string DeviceCategory { get; set; } = null!;
    public string DeviceSubCategory { get; set; } = null!;
    [JsonIgnore]
    public string EzDeviceCapability { get; set; } = null!;
    public string CustomType { get; set; } = null!;
    public string OfflineTime { get; set; } = null!;
    public int OfflineNotify { get; set; }
    public string InstructionBook { get; set; } = null!;
    public string AuthCode { get; set; } = null!;
    public string Username { get; set; } = null!;
    public int RiskLevel { get; set; }
    public long OfflineTimestamp { get; set; }
    public string Mac { get; set; } = null!;
    public int Classify { get; set; }
}
