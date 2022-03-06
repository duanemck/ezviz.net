namespace ezviz.net.domain.deviceInfo;
public class WeekPlan
{
    public string WeekDay { get; set; } = null!;
    public ICollection<TimeRange> TimePlan { get; set; } = null!;
}
