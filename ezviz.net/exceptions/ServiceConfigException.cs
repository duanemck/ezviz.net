using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.exceptions
{
    public class ServiceConfigException : Exception
    {
        public ServiceConfigException() : base("Could not get API service information")
        {
                
        }
    }
}
