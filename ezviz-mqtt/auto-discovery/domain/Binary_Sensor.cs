using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Binary_Sensor : Sensor
{
    public Binary_Sensor(string uniqueId, string name, Device device, string stateTopic) : base(uniqueId, name, device, stateTopic)
    {
    }
}
