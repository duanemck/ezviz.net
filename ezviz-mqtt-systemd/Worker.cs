using ezviz_mqtt;
using ezviz_mqtt.health;
using Microsoft.Extensions.Hosting.Systemd;

namespace ezviz_systemd.net
{
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
            _logger.LogInformation("IsSystemd: {isSystemd}", lifetime.GetType() == typeof(SystemdLifetime));
            _logger.LogInformation("IHostLifetime: {hostLifetime}", lifetime.GetType());
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
            serviceState.IsRunning = false;
        }
    }
}