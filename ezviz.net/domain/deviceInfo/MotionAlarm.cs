namespace ezviz.net.domain.deviceInfo
{
    public class MotionAlarm
    {
        public bool MotionTriggerActive { get; set; }
        public int SecondsSinceLastTrigger { get; set; }
        public DateTime LastAlarmTime { get; set; }
        public string LastAlarmPicture { get; set; } = null!;
        public string LastAlarmTypeCode { get; set; } = null!;
        public string LastAlarmTypeName { get; set; } = null!;
    }
}
