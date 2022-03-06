using ezviz_systemd.net;
using ezviz_systemd.net.config;


await Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(app =>
    {
        app.AddUserSecrets<Program>();
    })
    .ConfigureServices((hostContext,services) =>
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
        services.Configure<EzvizOptions>(hostContext.Configuration.GetSection("ezviz"));
        services.Configure<MqttOptions>(hostContext.Configuration.GetSection("mqtt"));
        services.Configure<JsonOptions>(hostContext.Configuration.GetSection("json"));

        services.AddSingleton<IMqttPublisher, MqttPublisher>();
    })
    .Build()
    .RunAsync();
