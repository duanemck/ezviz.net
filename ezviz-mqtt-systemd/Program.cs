using ezviz_mqtt;
using ezviz_systemd.net;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("/etc/ezviz-mqtt/appsettings.json", true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddSystemdLogging()
            .AddMqttPublisher<Worker>(hostContext.Configuration);
    })
    .UseSystemd()
    .Build()
    .RunAsync();
