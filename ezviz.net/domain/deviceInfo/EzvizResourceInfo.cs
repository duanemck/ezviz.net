namespace ezviz.net.domain.deviceInfo;

public class EzvizResourceInfo
{
    public string ResourceId { get; set; } = null!;
    public string ResourceName { get; set; } = null!;
    public string DeviceSerial { get; set; } = null!;
    public string SuperDeviceSerial { get; set; } = null!;
    public string LocalIndex { get; set; } = null!;
    public int ShareType { get; set; }
    public int Permission { get; set; }
    public int ResourceType { get; set; }
    public string ResourceCover { get; set; } = null!;
    public int IsShow { get; set; }
    public int VideoLevel { get; set; }
    public string StreamBizUrl { get; set; } = null!;
    public int GroupId { get; set; }
    public int CustomSetTag { get; set; }
}
