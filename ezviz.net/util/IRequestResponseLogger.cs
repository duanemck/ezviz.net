using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.util
{
    public interface IRequestResponseLogger
    {
        Task Log(Guid? id, string content);
    }
}
