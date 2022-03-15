namespace ezviz.net.domain.deviceInfo;

using ezviz.net.exceptions;
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
        if (cloud != null)
        {
            cloud.ResourceId = cloudObjectKeyValue.Key;
            Cloud = cloud;
            ResourceInfo = response.ResourceInfos.First(r => r.ResourceId == cloud.ResourceId);
            VTM = response.VTM == null ? new VTM() : Deserialize<VTM>(response.VTM[cloud.ResourceId]);
            Channel = response.Channel == null ? new Channel() : Deserialize<Channel>(response.Channel[cloud.ResourceId]);
            VideoQualities = response.Video_Quality == null? new VideoQuality[0] : Deserialize<VideoQuality[]>(response.Video_Quality[cloud.ResourceId]);
        }
        
        P2P = Deserialize<P2PEndpoint[]>(response.P2P[serial]);
        Connection = Deserialize<Connection>(response.Connection[serial]);
        KMS = Deserialize<KMS>(response.KMS[serial]);
        Status = Deserialize<Status>(response.Status[serial]);
        TimePlans = Deserialize<TimePlan[]>(response.Time_Plan[serial]);
        QOS = Deserialize<QOS>(response.QOS[serial]);
        NoDisturb = Deserialize<NoDisturb>(response.NoDisturb[serial]);
        Upgrade = Deserialize<Upgrade>(response.Upgrade[serial]);
        Switches = Deserialize<Switch[]>(response.Switch[serial]);
        Wifi = Deserialize<Wifi>(response.Wifi[serial]);
    }

    private T Deserialize<T>(JsonElement json)
    {
#pragma warning disable IL2026
        var result = json.Deserialize<T>( deserializationOptions);
#pragma warning restore IL2026
        if (result == null)
        {
            throw new EzvizNetException("Could not deserialize response");
        }
        return result;
    }

    public EzvizDeviceInfo DeviceInfo { get; set; }
    public EzvizResourceInfo ResourceInfo { get; set; } = null!;
    public ICollection<Switch> Switches { get; set; } = null!;
    public Wifi Wifi { get; set; } = null!;


    protected Cloud Cloud { get; set; } = null!;
    protected VTM VTM { get; set; } = null!;
    protected ICollection<P2PEndpoint> P2P { get; set; } = null!;
    protected Connection Connection { get; set; } = null!;
    protected KMS KMS { get; set; } = null!;
    protected Status Status { get; set; } = null!;

    protected ICollection<TimePlan>? TimePlans { get; set; } = null!;
    protected Channel Channel { get; set; } = null!;
    protected QOS QOS { get; set; } = null!;
    protected NoDisturb NoDisturb { get; set; } = null!;
    protected Upgrade Upgrade { get; set; } = null!;

    protected ICollection<VideoQuality> VideoQualities { get; set; } = null!;

}

