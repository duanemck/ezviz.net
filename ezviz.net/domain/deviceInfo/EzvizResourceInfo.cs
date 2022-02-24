namespace ezviz.net.domain.deviceInfo;

public class EzvizResourceInfo
{
    public string ResourceId { get; set; }
    public string ResourceName { get; set; }
    public string DeviceSerial { get; set; }
    public string SuperDeviceSerial { get; set; }
    public string LocalIndex { get; set; }
    public int ShareType { get; set; }
    public int Permission { get; set; }
    public int ResourceType { get; set; }
    public string ResourceCover { get; set; }
    public int IsShow { get; set; }
    public int VideoLevel { get; set; }
    public string StreamBizUrl { get; set; }
    public int GroupId { get; set; }
    public int CustomSetTag { get; set; }
}
