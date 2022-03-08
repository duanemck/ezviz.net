using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain
{
    public enum DetectionSensitivityLevel
    {
        Unknown = -2,
        Hibernate = -1,
        Off = 0,
        Lowest = 1,
        VeryLow = 2,
        Low = 3,
        High = 4,
        VeryHigh = 5,
        Highest
    }
}
