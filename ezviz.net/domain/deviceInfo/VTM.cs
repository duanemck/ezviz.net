using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class VTM
{
    public string Domain { get; set; }
    public string ExternalIp { get; set; }
    public string InternalIp { get; set; }
    public int Port { get; set; }
    public string Memo { get; set; }
    public int ForceStreamType { get; set; }
    public int IsBackup { get; set; }
    public PublicKey PublicKey { get; set; }
}

public class PublicKey
{
    public string Key { get; set; }
    public int Version { get; set; }
}

