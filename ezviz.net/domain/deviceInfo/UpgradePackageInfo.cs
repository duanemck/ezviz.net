namespace ezviz.net.domain.deviceInfo;

public class UpgradePackageInfo
{
    public string DevType { get; set; } = null!;
    public string FirmwareCode { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Platform { get; set; } = null!;
    public string Md5 { get; set; } = null!;
    public int Type { get; set; } = 0;
    public string PackageUrl { get; set; } = null!;
    public long PackageSize { get; set; } = 0;
    public bool IsGray { get; set; } = false;
    public string Desc { get; set; } = null!;
    public string Title { get; set; } = null!;

}