namespace ezviz.net.domain.deviceInfo;
public class WeekPlan
{
    public string WeekDay { get; set; }
    public ICollection<TimeRange> TimePlan { get; set; }
}
