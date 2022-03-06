using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class VTM
{
    public string Domain { get; set; } = null!;
    public string ExternalIp { get; set; } = null!;
    public string InternalIp { get; set; } = null!;
    public int Port { get; set; }
    public string Memo { get; set; } = null!;
    public int ForceStreamType { get; set; }
    public int IsBackup { get; set; }
    public PublicKey PublicKey { get; set; } = null!;
}

public class PublicKey
{
    public string Key { get; set; } = null!;
    public int Version { get; set; }
}

