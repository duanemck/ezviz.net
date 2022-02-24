namespace ezviz.net.domain;

public class EzvizUser
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string ConfusedPhone { get; set; }
    public string ConfusedEmail { get; set; }
    public string CustomNo { get; set; }
    public int AreaId { get; set; }
    public bool NeedTrans { get; set; }
    public bool TransferringToStandaloneRegion { get; set; }
    public string UserCode { get; set; }
    public string AvatarPath { get; set; }
    public string Contact { get; set; }
    public int Category { get; set; }
    public string HomeTitle { get; set; }
    public string Location { get; set; }
    public string RegDate { get; set; }
    public string LangType { get; set; }
    public int MsgStatus { get; set; }
}
