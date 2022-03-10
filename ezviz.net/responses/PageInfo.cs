namespace ezviz.net.domain;
public class PageInfo
{
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalResults { get; set; }
    public bool HasNext { get; set; }

}
