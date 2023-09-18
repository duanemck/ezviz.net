using ezviz.net.util;

namespace ezviz_cli
{
    internal class DefaultRequestResponseLogger : IRequestResponseLogger
    {
        public Task Log(Guid? id, string? serialNumber, string message)
        {
            Console.WriteLine($"[{serialNumber ?? id.ToString()}] {message}");
            return Task.CompletedTask; 
        }
    }
}
