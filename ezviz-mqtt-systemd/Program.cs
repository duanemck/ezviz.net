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
        LogLevel logLevel = EnumX.Parse<LogLevel>(hostContext.Configuration.GetSection("log")["LogLevel"] ?? "Information");
        services
            .AddLogging(config =>
            {
                config
                    .ClearProviders()
                    .AddProvider(new CustomLoggerProvider())
                    .AddFile(hostContext.Configuration.GetSection("log"))
                    .AddFilter("Microsoft", LogLevel.None)
                    .AddFilter("System", LogLevel.None);
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = logLevel)
            //.AddSystemdLogging()
            .AddMqttWorker<Worker>(hostContext.Configuration);
    })
    .UseSystemd()
    .Build()
    .RunAsync();
