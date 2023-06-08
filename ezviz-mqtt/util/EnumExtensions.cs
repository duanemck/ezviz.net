namespace ezviz_mqtt.util;

public static class EnumX
{
    public static T ToObject<T>(int ordinalValue)
    {
        return (T)Enum.ToObject(typeof(T), ordinalValue);
    }

    public static T Parse<T>(string name)
    {
        return (T)Enum.Parse(typeof(T), name);
    }
}

