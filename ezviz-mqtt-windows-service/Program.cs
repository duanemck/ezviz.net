using ezviz_mqtt;
using ezviz_mqtt.util;
using ezviz_windows_service;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ezviz-mqtt");
        app.AddJsonFile(Path.Combine(folder, "appsettings.json"), true);
        app.AddUserSecrets<Program>();
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
            .AddWindowsLogging()
            .AddMqttWorker<Worker>(hostContext.Configuration);
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "ezviz-mqtt";
    })
    .Build()
    .RunAsync();
