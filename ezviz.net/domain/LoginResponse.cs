using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;

internal class LoginResponse
{
    public bool Isolate { get; set; }
    public LoginTerminalStatus LoginTerminalStatus { get; set; }
    public EzvizUser LoginUser { get; set; }
    public bool HcGvIsolate { get; set; }
    public string TelephoneCode { get; set; }
    public Meta Meta { get; set; }
    public LoginArea LoginArea { get; set; }
    public LoginSession LoginSession { get; set; }
}

internal class LoginTerminalStatus
{
    public string TerminalBinded { get; set; }
    public string TerminalOpened { get; set; }
}

internal class LoginArea
{
    public string ApiDomain { get; set; }
    public string WebDomain { get; set; }
    public string AreaName { get; set; }
    public int AreaId { get; set; }
}

internal class LoginSession
{
    public string SessionId { get; set; }
    public string RfSessionId { get; set; }
}