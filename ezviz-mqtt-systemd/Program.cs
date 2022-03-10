using ezviz_mqtt;
using ezviz_systemd.net;


await Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("/etc/ezviz-mqtt/appsettings.json", true);
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
        services.AddMqttPublisher(hostContext.Configuration);
    })
    .Build()
    .RunAsync();
