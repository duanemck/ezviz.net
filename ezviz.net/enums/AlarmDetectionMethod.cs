using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    public enum AlarmDetectionMethod
    {
        Unknown = -1,
        HumanOrVehicle = 0,
        Human = 1,
        Vehicle = 2,
        ImageChange = 3
    }
}
