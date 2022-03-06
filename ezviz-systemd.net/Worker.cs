using Microsoft.Extensions.Hosting.Systemd;

namespace ezviz_systemd.net
{
    public class Worker : BackgroundService
    {

        private const int PollingFrequencyMinutes = 5;
        private readonly ILogger<Worker> _logger;
        private readonly IMqttPublisher publisher;

        public Worker(ILogger<Worker> logger, IHostLifetime lifetime, IMqttPublisher publisher)
        {
            _logger = logger;
            this.publisher = publisher;
            _logger.LogInformation("IsSystemd: {isSystemd}", lifetime.GetType() == typeof(SystemdLifetime));
            _logger.LogInformation("IHostLifetime: {hostLifetime}", lifetime.GetType());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await publisher.Init();
            DateTime lastPoll = default(DateTime);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.Now - lastPoll >= new TimeSpan(0, PollingFrequencyMinutes, 0))
                {
                    await publisher.PublishAsync();

                    lastPoll = DateTime.Now;
                }
                await Task.Delay(500, stoppingToken);
            }
        }       
    }
}