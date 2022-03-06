using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class Status
{
    public int DiskNum { get; set; }
    public string DiskState { get; set; } = null!;
    public int GlobalStatus { get; set; }
    public int PirStatus { get; set; }
    public int IsEncrypt { get; set; }
    public string EncryptPwd { get; set; } = null!;
    public int UpgradeAvailable { get; set; }
    public int UpgradeProcess { get; set; }
    public int UpgradeStatus { get; set; }
    public AlarmSound AlarmSoundMode { get; set; }

    public IDictionary<string, string> Optionals { get; set; } = new Dictionary<string, string>();

}

