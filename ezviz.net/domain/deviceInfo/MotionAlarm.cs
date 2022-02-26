namespace ezviz.net.domain.deviceInfo
{
    public class MotionAlarm
    {
        public bool MotionTriggerActive { get; set; }
        public int SecondsSinceLastTrigger { get; set; }
        public DateTime LastAlarmTime { get; set; }
        public string LastAlarmPicture { get; set; }
        public string LastAlarmTypeCode { get; set; }
        public string LastAlarmTypeName { get; set; }
    }
}
