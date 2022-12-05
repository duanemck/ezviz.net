namespace ezviz.net.domain.deviceInfo;

public class Upgrade
{
    public int IsNeedUpgrade { get; set; }
    public UpgradePackageInfo UpgradePackageInfo { get; set; } = null!;
}
