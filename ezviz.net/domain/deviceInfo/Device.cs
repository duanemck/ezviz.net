namespace ezviz.net.domain.deviceInfo;
using System.Text.Json;

public class Device
{
    private readonly JsonSerializerOptions deserializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    internal Device(EzvizDeviceInfo deviceInfo, PagedListResponse response)
    {
        DeviceInfo = deviceInfo;
        var serial = deviceInfo.DeviceSerial;
        var cloudObjectKeyValue = response.Cloud
                                .Where(c => c.Value.GetProperty("deviceSerial").GetString() == serial)
                                .FirstOrDefault();
        var cloud = Deserialize<Cloud>(cloudObjectKeyValue.Value);
        cloud.ResourceId = cloudObjectKeyValue.Key;
        Cloud = cloud;
        ResourceInfo = response.ResourceInfos.First(r => r.ResourceId == cloud.ResourceId);
        VTM = Deserialize<VTM>(response.VTM[cloud.ResourceId]);
        P2P = Deserialize<P2PEndpoint[]>(response.P2P[serial]);
        Connection = Deserialize<Connection>(response.Connection[serial]);
        KMS = Deserialize<KMS>(response.KMS[serial]);
        Status = Deserialize<Status>(response.Status[serial]);
        TimePlans = Deserialize<TimePlan[]>(response.Time_Plan[serial]);
        Channel = Deserialize<Channel>(response.Channel[cloud.ResourceId]);
        QOS = Deserialize<QOS>(response.QOS[serial]);
        NoDisturb = Deserialize<NoDisturb>(response.NoDisturb[serial]);
        Upgrade = Deserialize<Upgrade>(response.Upgrade[serial]);
        Switches = Deserialize<Switch[]>(response.Switch[serial]);
        VideoQualities = Deserialize<VideoQuality[]>(response.Video_Quality[cloud.ResourceId]);
        Wifi = Deserialize<Wifi>(response.Wifi[serial]);
    }

    private T Deserialize<T>(JsonElement json)
    {
        return json.Deserialize<T>(deserializationOptions);
    }

    public EzvizDeviceInfo DeviceInfo { get; set; }
    public EzvizResourceInfo ResourceInfo { get; set; }
    protected Cloud Cloud { get; set; }
    protected VTM VTM { get; set; }
    protected ICollection<P2PEndpoint> P2P { get; set; }
    protected Connection Connection { get; set; }
    protected KMS KMS { get; set; }
    protected Status Status { get; set; }

    protected ICollection<TimePlan> TimePlans { get; set; }
    protected Channel Channel { get; set; }
    protected QOS QOS { get; set; }
    protected NoDisturb NoDisturb { get; set; }
    protected Upgrade Upgrade { get; set; }
    public ICollection<Switch> Switches { get; set; }
    protected ICollection<VideoQuality> VideoQualities { get; set; }
    protected Wifi Wifi { get; set; }
}

