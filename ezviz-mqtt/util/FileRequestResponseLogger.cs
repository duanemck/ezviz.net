using ezviz.net.util;
using ezviz_mqtt.config;
using Microsoft.Extensions.Options;

namespace ezviz_mqtt.util
{
    internal class FileRequestResponseLogger : IRequestResponseLogger
    {
        private readonly PollingOptions pollingOptions;

        public FileRequestResponseLogger(IOptions<PollingOptions> pollingOptions)
        {
            this.pollingOptions = pollingOptions.Value;
        }

        public async Task Log(Guid id, string? serialNumber, string message)
        {
            await LogWithPrefix(id.ToString(), serialNumber, message);            
        }

        public async Task Log(string name, string? serialNumber, string message)
        {
            await LogWithPrefix(name, serialNumber, message);
        }

        private async Task LogWithPrefix(string prefix, string? serialNumber, string message)
        {
            if (string.IsNullOrEmpty(pollingOptions.RequestLogLocation))
            {
                return;
            }

            if (pollingOptions.LogResponsesForDevice == "*" || pollingOptions.LogResponsesForDevice == "" || pollingOptions.LogResponsesForDevice == serialNumber)
            {
                var fileName = Path.Join(pollingOptions.RequestLogLocation, $"{prefix}_{serialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                Directory.CreateDirectory(pollingOptions.RequestLogLocation);
                var file = File.CreateText(fileName);
                await file.WriteLineAsync(message);
                file.Close();
            }
        }
    }
}
