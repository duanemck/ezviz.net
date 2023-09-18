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

        public async Task Log(Guid? id, string? serialNumber, string message)
        {
            if (string.IsNullOrEmpty(pollingOptions.RequestLogLocation))
            {
                return;
            }

            if (pollingOptions.LogResponsesForDevice == "*" || pollingOptions.LogResponsesForDevice == "" || pollingOptions.LogResponsesForDevice == serialNumber)
            {
                var fileName = Path.Join(pollingOptions.RequestLogLocation, $"{serialNumber ?? id.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                Directory.CreateDirectory(pollingOptions.RequestLogLocation);
                var file = File.CreateText(fileName);
                await file.WriteLineAsync(message);
                file.Close();
            }
        }
    }
}
