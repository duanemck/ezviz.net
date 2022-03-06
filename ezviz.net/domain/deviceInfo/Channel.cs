namespace ezviz.net.domain.deviceInfo;

public class Channel
{
    public string ChannelDeviceSerial { get; set; } = null!;
    public int ChannelNo { get; set; }
    public int PrivacyStatus { get; set; }
    public int PowerStatus { get; set; }
    public int GlobalStatus { get; set; }
    public int SignalStatus { get; set; }
}

