using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz.net.util
{
    internal class DefaultRequestResponseLogger : IRequestResponseLogger
    {
        public Task Log(Guid? id, string message)
        {
            Console.WriteLine($"[{id}] {message}");
            return Task.CompletedTask; 
        }
    }
}
