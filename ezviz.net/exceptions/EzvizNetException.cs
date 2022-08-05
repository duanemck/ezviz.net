using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.exceptions
{
    public class EzvizNetException : Exception
    {
        public EzvizNetException(string message, Guid? id = null) : base($"[{id ?? Guid.NewGuid()}] {message}")
        {
            Id = id;
        }

        public EzvizNetException(string message, Exception? inner, Guid? id = null) : base($"[{id ?? Guid.NewGuid()}] {message}", inner)
        {
            Id = id;
        }

        public Guid? Id { get; }
    }
}
