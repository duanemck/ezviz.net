using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;
internal class ServiceUrlsResponse
{
    public SystemConfigInfo SystemConfigInfo { get; set; }
    public Meta Meta { get; set; }

    
}

internal class SystemConfigInfo
{
    private string sysConf;

    public int Minute { get; set; }
    public int Second { get; set; }
    public int EnableP2P { get; set; }
    public int Https { get; set; }
    public string UserCode { get; set; }
    public string Stun1Addr { get; set; }
    public int Stun1Port { get; set; }
    public string Stun2Addr { get; set; }
    public int Stun2Port { get; set; }
    public string TtsAddr { get; set; }
    public int TtsPort { get; set; }
    public string VtmAddr { get; set; }
    public int VtmPort { get; set; }
    public string PushAddr { get; set; }
    public int PushHttpPort { get; set; }
    public int PushHttpsPort { get; set; }
    public string AuthAddr { get; set; }
    public string DevicePicDomain { get; set; }
    public string NodeJsAddr { get; set; }
    public int NodeJsHttpPort { get; set; }
    public int CloudExpiredTipTime { get; set; }
    public int DeviceUpgradeTipTime { get; set; }


    public ICollection<string> ServiceUrls { get; private set; }
    public string SysConf
    {
        get => sysConf; set
        {
            sysConf = value;
            ServiceUrls = value.Split("|");
        }
    }
    
    public string NewTtsAddr { get; set; }
    public int NewTtsPort { get; set; }
    public string DclogUrl { get; set; }
    public string DclogHttpsUrl { get; set; }
    public string PmsAddr { get; set; }
    public int PmsPort { get; set; }
    public string PmsHttpsAddr { get; set; }
    public int PmsHttpsPort { get; set; }
    public int P2pCheckInterval { get; set; }
    public string EzvizEvaluationVersion { get; set; }
    public string PushDasDomain { get; set; }
    public int PushDasPort { get; set; }
    public int Ssp { get; set; }
}

