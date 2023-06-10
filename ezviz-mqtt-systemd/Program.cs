using ezviz_mqtt;
using ezviz_mqtt.util;
using ezviz_systemd.net;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("/etc/ezviz-mqtt/appsettings.json", true);
        app.AddJsonFile("/config/appsettings.json", true);
        app.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var loggingConfig = hostContext.Configuration.GetSection("log");
        LogLevel logLevel = EnumX.Parse<LogLevel>(loggingConfig["LogLevel"] ?? "Information");
        services
            .AddLogging(config =>
            {
                config
                    .ClearProviders()
                    .AddProvider(new CustomLoggerProvider())
                    .AddFilter("Microsoft", LogLevel.None)
                    .AddFilter("System", LogLevel.None);

                if (loggingConfig != null)
                {
                    config.AddFile(loggingConfig);
                }
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = logLevel)
            .AddSystemdLogging()
            .AddMqttWorker<Worker>(hostContext.Configuration);
    })
    .UseSystemd()
    .Build()
    .RunAsync();
