using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class Connection
{
    public string LocalIp { get; set; }
    public string NetIp { get; set; }
    public int LocalRtspPort { get; set; }
    public int NetRtspPort { get; set; }
    public int LocalCmdPort { get; set; }
    public int NetCmdPort { get; set; }
    public int LocalStreamPort { get; set; }
    public int NetHttpPort { get; set; }
    public int LocalHttpPort { get; set; }
    public int NetStreamPort { get; set; }
    public int NetType { get; set; }
    public string WanIp { get; set; }
    public bool UPnp { get; set; }
}

