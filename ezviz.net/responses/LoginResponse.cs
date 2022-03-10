using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ezviz.net.domain;

internal class LoginResponse : GenericResponse
{
    public bool Isolate { get; set; }
    public LoginTerminalStatus LoginTerminalStatus { get; set; } = null!;
    public EzvizUser LoginUser { get; set; } = null!;
    public bool HcGvIsolate { get; set; }
    public string TelephoneCode { get; set; } = null!;
    public LoginArea LoginArea { get; set; } = null!;
    public LoginSession LoginSession { get; set; } = null!;
}

internal class LoginTerminalStatus
{
    public string TerminalBinded { get; set; } = null!;
    public string TerminalOpened { get; set; } = null!;
}

internal class LoginArea
{
    public string ApiDomain { get; set; } = null!;
    public string WebDomain { get; set; } = null!;
    public string AreaName { get; set; } = null!;
    public int AreaId { get; set; }
}

internal class LoginSession
{
    public string SessionId { get; set; } = null!;
    public string RfSessionId { get; set; } = null!;
    [JsonIgnore]
    public DateTime SessionExpiry { get; set; }
}