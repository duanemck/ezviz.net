namespace ezviz_mqtt.config;
internal class PollingOptions
{
    public int Cameras { get; set; }
    public int Alarms { get; set; }
    public int AlarmStaleAgeMinutes { get; set; }

    public string? RequestLogLocation { get; set; }
}

