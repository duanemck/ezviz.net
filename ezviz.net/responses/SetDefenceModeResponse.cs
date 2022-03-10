using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    internal class SetDefenceModeResponse : GenericResponse
    {
        public IDictionary<string, string> RetMap { get; set; } = null!;
    }

    internal class GetDefenceModeResponse : GenericResponse
    {
        public string Mode { get; set; } = null!;
    }
}
