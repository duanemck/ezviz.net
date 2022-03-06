using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;
internal class ServiceUrlsResponse : GenericResponse
{
    public SystemConfigInfo SystemConfigInfo { get; set; } = null!;

}

internal class SystemConfigInfo
{
    private string sysConf = null!;

    public int Minute { get; set; }
    public int Second { get; set; }
    public int EnableP2P { get; set; }
    public int Https { get; set; }
    public string UserCode { get; set; } = null!;
    public string Stun1Addr { get; set; } = null!;
    public int Stun1Port { get; set; }
    public string Stun2Addr { get; set; } = null!;
    public int Stun2Port { get; set; }
    public string TtsAddr { get; set; } = null!;
    public int TtsPort { get; set; }
    public string VtmAddr { get; set; } = null!;
    public int VtmPort { get; set; }
    public string PushAddr { get; set; } = null!;
    public int PushHttpPort { get; set; }
    public int PushHttpsPort { get; set; }
    public string AuthAddr { get; set; } = null!;
    public string DevicePicDomain { get; set; } = null!;
    public string NodeJsAddr { get; set; } = null!;
    public int NodeJsHttpPort { get; set; }
    public int CloudExpiredTipTime { get; set; }
    public int DeviceUpgradeTipTime { get; set; }

    public ICollection<string> ServiceUrls { get; private set; } = null!;
    public string SysConf
    {
        get => sysConf; set
        {
            sysConf = value;
            ServiceUrls = value.Split("|");
        }
    }
    
    public string NewTtsAddr { get; set; } = null!;
    public int NewTtsPort { get; set; }
    public string DclogUrl { get; set; } = null!;
    public string DclogHttpsUrl { get; set; } = null!;
    public string PmsAddr { get; set; } = null!;
    public int PmsPort { get; set; }
    public string PmsHttpsAddr { get; set; } = null!;
    public int PmsHttpsPort { get; set; }
    public int P2pCheckInterval { get; set; }
    public string EzvizEvaluationVersion { get; set; } = null!;
    public string PushDasDomain { get; set; } = null!;
    public int PushDasPort { get; set; }
    public int Ssp { get; set; }
}

