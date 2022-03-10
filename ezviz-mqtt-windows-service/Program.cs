using ezviz_mqtt;
using ezviz_windows_service;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"ezviz-mqtt");
        app.AddJsonFile(Path.Combine(folder,"appsettings.json"), true);
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "ezviz-mqtt";
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
