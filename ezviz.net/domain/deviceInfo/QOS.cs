namespace ezviz.net.domain.deviceInfo;

public class QOS
{
    public string Domain { get; set; } = null!;
    public string ExternalIp { get; set; } = null!;
    public string InternalIp { get; set; } = null!;
    public int Port { get; set; }
    public string Memo { get; set; } = null!;
    public string IdcType { get; set; } = null!;
    public int IsBackup { get; set; }
}

