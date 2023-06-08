namespace ezviz.net.cloud_mqtt;
internal class PushMessage
{
    public long Id { get; set; }
    public string? Alert { get; set; }
    public string? Ext { get; set; }

    public long T { get; set; }
    public string? Sound { get; set; }
    public int Badge { get; set; }

    public Dictionary<string,object>? Extras{ get; set; }
}



