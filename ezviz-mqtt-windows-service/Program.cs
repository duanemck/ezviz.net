using ezviz_mqtt;
using ezviz_mqtt.util;
using EnumX = ezviz_mqtt.util.EnumX;
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
        LogLevel logLevel = EnumX.Parse<LogLevel>(hostContext.Configuration.GetSection("Logging")["LogLevel"] ?? "Information");
        services
            .AddWindowsLogging()
            .AddLogging(config =>
            {
                
                config
                    .ClearProviders()
                    .AddProvider(new CustomLoggerProvider())
                    .AddFile(hostContext.Configuration.GetSection("Logging"))
                    .AddFilter("Microsoft", LogLevel.None)
                    .AddFilter("System", LogLevel.None);
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = logLevel)
            .AddMqttPublisher<Worker>(hostContext.Configuration);            
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "ezviz-mqtt";
    })
    .Build()
    .RunAsync();
