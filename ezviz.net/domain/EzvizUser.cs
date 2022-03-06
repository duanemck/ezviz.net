namespace ezviz.net.domain;

public class EzvizUser
{
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string ConfusedPhone { get; set; } = null!;
    public string ConfusedEmail { get; set; } = null!;
    public string CustomNo { get; set; } = null!;
    public int AreaId { get; set; }
    public bool NeedTrans { get; set; }
    public bool TransferringToStandaloneRegion { get; set; }
    public string UserCode { get; set; } = null!;
    public string AvatarPath { get; set; } = null!;
    public string Contact { get; set; } = null!;
    public int Category { get; set; }
    public string HomeTitle { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string RegDate { get; set; } = null!;
    public string LangType { get; set; } = null!;
    public int MsgStatus { get; set; }
}
