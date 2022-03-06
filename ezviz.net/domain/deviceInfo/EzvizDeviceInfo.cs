using System.Text.Json.Serialization;

namespace ezviz.net.domain.deviceInfo;

public class EzvizDeviceInfo
{
    public string Name { get; set; }
    public string DeviceSerial { get; set; }
    public string FullSerial { get; set; }
    public string DeviceType { get; set; }
    public string DevicePicPrefix { get; set; }
    public string Version { get; set; }
    [JsonIgnore]
    public string SupportExt { get; set; }
    public int Status { get; set; }
    public string UserDeviceCreateTime { get; set; }
    public int ChannelNumber { get; set; }
    public bool Hik { get; set; }
    public string DeviceCategory { get; set; }
    public string DeviceSubCategory { get; set; }
    [JsonIgnore]
    public string EzDeviceCapability { get; set; }
    public string CustomType { get; set; }
    public string OfflineTime { get; set; }
    public int OfflineNotify { get; set; }
    public string InstructionBook { get; set; }
    public string AuthCode { get; set; }
    public string Username { get; set; }
    public int RiskLevel { get; set; }
    public long OfflineTimestamp { get; set; }
    public string Mac { get; set; }
    public int Classify { get; set; }
}
