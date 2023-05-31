using ezviz_mqtt;
using ezviz_mqtt.health;

namespace ezviz_windows_service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttPublisher publisher;
    private readonly MqttServiceState serviceState;

    public Worker(ILogger<Worker> logger, IHostLifetime lifetime, IMqttPublisher publisher, MqttServiceState serviceState)
    {
        _logger = logger;
        this.publisher = publisher;
        this.serviceState = serviceState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.Init();
        while (!stoppingToken.IsCancellationRequested)
        {
            serviceState.IsRunning = true;
            await publisher.PublishAsync(stoppingToken);
            await Task.Delay(500, stoppingToken);
        }
        await publisher.Shutdown();
        serviceState.IsRunning = false;
    }
}
