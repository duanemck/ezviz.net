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

        public async Task Log(Guid? id, string message)
        {
            if (string.IsNullOrEmpty(pollingOptions.RequestLogLocation))
            {
                return;
            }
            var fileName = Path.Join(pollingOptions.RequestLogLocation, $"{id.ToString()}.txt");
            var file = File.CreateText(fileName);
            await file.WriteLineAsync(message);
            file.Close();                       
        }
    }
}
