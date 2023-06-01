using ezviz.net.util;

namespace ezviz_cli
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
