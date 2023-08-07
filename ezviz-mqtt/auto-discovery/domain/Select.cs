using ezviz_mqtt.util;
using System.Text.Json.Serialization;

namespace ezviz_mqtt.auto_discovery.domain;

public class Select : Entity
{
    public Select(string uniqueId, string name, Device device, string commandTopic, string stateTopic, Type enumType) : this(uniqueId, name, device, commandTopic, stateTopic, Enum.GetNames(enumType))
    {

    }

    public Select(string uniqueId, string name, Device device, string commandTopic, string stateTopic, IEnumerable<string> options) : base(uniqueId, name, device)
    {
        CommandTopic = commandTopic;
        StateTopic = stateTopic;
        Options = options;
        EntityCategory = "config";
    }

    [JsonPropertyName("command_topic")]
    public string CommandTopic { get; }
    
    [JsonPropertyName("state_topic")]
    public string StateTopic { get; }
    
    public IEnumerable<string> Options { get; }
}
