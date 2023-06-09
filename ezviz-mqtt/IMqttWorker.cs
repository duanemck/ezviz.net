namespace ezviz_mqtt;

public interface IMqttWorker : IDisposable
{
    Task PublishAsync(CancellationToken stoppingToken, bool force = false);
    Task Init();
    Task Shutdown();
}

