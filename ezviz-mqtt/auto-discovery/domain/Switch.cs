using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Switch : Entity
{
    public Switch(string uniqueId, string name, Device device, string commandTopic, string stateTopic) : base(uniqueId, name, device)
    {
        CommandTopic = commandTopic;
        StateTopic = stateTopic;
        EntityCategory = "config";
    }

    [JsonPropertyName("command_topic")]
    public string CommandTopic { get; }
    
    [JsonPropertyName("state_topic")]
    public string StateTopic { get; }
    
}
