using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_mqtt.util;

internal static class EnumX
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

