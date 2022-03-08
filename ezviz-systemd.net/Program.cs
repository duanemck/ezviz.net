using ezviz_systemd.net;
using ezviz_systemd.net.config;


await Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("/etc/ezviz/appsettings.json", true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(builder =>
        {
            builder.AddSystemdConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "HH:mm:ss";
            });
        });
        services.AddHostedService<Worker>();
        services.AddOptions();
#pragma warning disable IL2026
        services.Configure<EzvizOptions>(hostContext.Configuration.GetSection("ezviz"));
        services.Configure<MqttOptions>(hostContext.Configuration.GetSection("mqtt"));
        services.Configure<JsonOptions>(hostContext.Configuration.GetSection("json"));
        services.Configure<PollingOptions>(hostContext.Configuration.GetSection("polling"));
#pragma warning restore IL2026
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
    })
    .Build()
    .RunAsync();
