using ezviz.net.util;
using System.Text.Json.Serialization;

namespace ezviz.net.domain.deviceInfo;

public class Switch
{
    public string DeviceSerial { get; set; } = null!;
    public int ChannelNo { get; set; }
    public SwitchType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool Enable { get; set; }
}

