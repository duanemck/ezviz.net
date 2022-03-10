using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezviz_mqtt;

public interface IMqttPublisher : IDisposable
{
    Task PublishAsync(CancellationToken stoppingToken);
    Task Init();
}

