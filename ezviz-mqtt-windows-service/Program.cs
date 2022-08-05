using ezviz_mqtt;
using ezviz_windows_service;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(app =>
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ezviz-mqtt");
        app.AddJsonFile(Path.Combine(folder, "appsettings.json"), true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddWindowsLogging()            
            .AddMqttPublisher<Worker>(hostContext.Configuration);
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "ezviz-mqtt";
    })
    .Build()
    .RunAsync();
