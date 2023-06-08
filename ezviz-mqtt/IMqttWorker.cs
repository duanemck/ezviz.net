namespace ezviz_mqtt;

public interface IMqttWorker : IDisposable
{
    Task PublishAsync(CancellationToken stoppingToken);
    Task Init();
    Task Shutdown();
}

