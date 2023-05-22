using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ezviz_mqtt.home_assistant;

internal class Entity
{
    public string Name { get; set; }

    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; }

    public Device Device { get; set; }

    [JsonPropertyName("state_topic")]
    public string StateTopic { get; set; }

    [JsonPropertyName("command_topic")]
    public string CommandTopic { get; set; }
    public Availability Availability { get; set; }


    [JsonPropertyName("device_class")]
    public string DeviceClass { get; set; }

    [JsonPropertyName("payload_on")]
    public string PayloadOn { get; set; }
    [JsonPropertyName("payload_off")]
    public string PayloadOff { get; set; }

}

