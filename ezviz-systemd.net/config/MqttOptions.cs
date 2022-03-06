using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_systemd.net.config
{
    public class MqttOptions
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public IDictionary<string,string> Topics { get; set; }
    }
}
