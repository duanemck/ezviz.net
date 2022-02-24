using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.exceptions
{
    public class EzvizNetException : Exception
    {
        public EzvizNetException(string message) : base(message)
        {

        }

        public EzvizNetException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
