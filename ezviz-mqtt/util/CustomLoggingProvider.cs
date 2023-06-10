using System.Text;
using System.Drawing;
using System.Globalization;
using Microsoft.Extensions.Logging;

using Pastel;

namespace ezviz_mqtt.util
{


    public class CustomLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly Dictionary<LogLevel, string> _shortLevels = new Dictionary<LogLevel, string>()
            {
                {LogLevel.Critical, "CRIT " },
                {LogLevel.Debug, "DEBUG" },
                {LogLevel.Error, "ERROR" },
                {LogLevel.Information, "INFO " },
                {LogLevel.Trace, "TRACE" },
                {LogLevel.Warning, "WARN " }
            };

            private readonly string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                StringBuilder message = new StringBuilder();
                message.Append("[").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Pastel(Color.DarkOrange)).Append("]");
                message.Append("[").Append(_shortLevels[logLevel].Pastel(Color.DarkMagenta)).Append("]");
                message.Append("[").Append(_categoryName.PadRight(35).Pastel(Color.DarkGreen)).Append("]");
                message.AppendLine(formatter(state, exception).Pastel(Color.White));

                if (exception != null)
                {
                    message.AppendLine(exception.Message.PastelBg(Color.Red));
                    message.AppendLine(exception.StackTrace.PastelBg(Color.Red));
                    Console.Error.Write(message);
                }
                else
                {
                    Console.Out.Write(message);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
            }
        }
    }
}

