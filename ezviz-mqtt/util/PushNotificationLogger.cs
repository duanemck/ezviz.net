using ezviz.net.util;
using Microsoft.Extensions.Logging;

namespace ezviz_mqtt.util
{
    internal class PushNotificationLogger : IPushNotificationLogger
    {
        private readonly ILogger<PushNotificationLogger> logger;

        public PushNotificationLogger(ILogger<PushNotificationLogger> logger)
        {
            this.logger = logger;
        }


        public void LogInformation(string content, params object[] formatParams)
        {
            logger.LogInformation(content, formatParams);
        }
    }
}
