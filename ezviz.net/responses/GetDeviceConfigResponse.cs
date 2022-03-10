using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    internal class GetDeviceConfigResponse : GenericResponse
    {
        public string ValueInfo { get; set; } = null!;
    }
}
