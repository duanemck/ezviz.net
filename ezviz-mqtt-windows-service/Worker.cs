using ezviz_mqtt;

namespace ezviz_windows_service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttPublisher publisher;

    public Worker(ILogger<Worker> logger, IHostLifetime lifetime, IMqttPublisher publisher)
    {
        _logger = logger;
        this.publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await publisher.Init();
        while (!stoppingToken.IsCancellationRequested)
        {
            await publisher.PublishAsync(stoppingToken);
            await Task.Delay(500, stoppingToken);
        }
    }
}
