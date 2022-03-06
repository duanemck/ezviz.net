namespace ezviz.net.domain.deviceInfo;

public class Wifi
{
    public string NetName { get; set; } = null!;
    public string NetType { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Mask { get; set; } = null!;
    public string Gateway { get; set; } = null!;
    public int Signal { get; set; }
    public string SSID { get; set; } = null!;
}

