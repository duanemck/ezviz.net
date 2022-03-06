using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_systemd.net.config
{
    public enum BooleanSerializationTypes
    {
        Default = 0,
        String = 1,
        Numbers = 2
    }

    public class JsonOptions
    {
        public BooleanSerializationTypes SerializeBooleans { get; set; }
        public string SerializeTrueAs { get; set; } = null!;     
        public string SerializeFalseAs { get; set; } = null!;

    }
}
