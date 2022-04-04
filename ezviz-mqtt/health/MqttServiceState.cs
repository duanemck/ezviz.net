using System.Net;
using System.Text.Json.Serialization;

namespace ezviz_mqtt.health;

public class MqttServiceState
{
    [JsonIgnore]
    public HttpStatusCode StatusCode => IsRunning && MqttConnected ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

    public bool IsRunning { get; set; }
    public bool MqttConnected { get; set; }
    public DateTime LastStatusCheck { get; set; }
    public DateTime LastAlarmCheck { get; set; }
    public string? MostRecentError { get; set; }
    public DateTime? MostRecentErrorTime { get; set; }

}

