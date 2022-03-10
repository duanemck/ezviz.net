namespace ezviz_mqtt.config;
internal enum BooleanSerializationTypes
{
    Default = 0,
    String = 1,
    Numbers = 2
}

internal class JsonOptions
{
    public BooleanSerializationTypes SerializeBooleans { get; set; }
    public string SerializeTrueAs { get; set; } = null!;
    public string SerializeFalseAs { get; set; } = null!;

}

