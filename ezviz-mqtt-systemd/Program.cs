using ezviz_mqtt;
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
        services
            .AddSystemdLogging()
            .AddLogging(c=>
            {
                c.AddFile(hostContext.Configuration.GetSection("Logging"));
            })
            .AddMqttPublisher<Worker>(hostContext.Configuration);
    })
    .UseSystemd()
    .Build()
    .RunAsync();
