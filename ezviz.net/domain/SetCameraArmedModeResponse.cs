using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    internal class SetCameraArmedModeResponse : GenericResponse
    {
        public string DefenceStatus { get; set; } = null!;
    }
}
