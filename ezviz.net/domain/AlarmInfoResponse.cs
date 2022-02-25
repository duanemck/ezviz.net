using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.domain;

internal class AlarmInfoResponse
{
    public Meta Meta { get; set; }
    public PageInfo Page { get; set; }
    public ICollection<Alarm> Alarms { get; set; }
}

