using System.Text.Json;

namespace ezviz.net.domain.deviceInfo;

public class TimePlan
{
    public string DeviceSerial { get; set; } = null!;

    public int ChannelNo { get; set; }
    public int Type { get; set; }
    public int Enable { get; set; }
    public ICollection<WeekPlan> WeekPlans { get; set; } = null!;
}

