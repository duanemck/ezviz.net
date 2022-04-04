using Microsoft.Extensions.Logging;
using System.Text.Json;
using TinyHealthCheck.HealthChecks;
using TinyHealthCheck.Models;

namespace ezviz_mqtt.health;

public class MqttHealthCheck : IHealthCheck
{
    private readonly MqttServiceState serviceState;

    public MqttHealthCheck( MqttServiceState serviceState)
    {        
        this.serviceState = serviceState;
    }

    public async Task<IHealthCheckResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        return new HealthCheckResult(JsonSerializer.Serialize(serviceState), serviceState.StatusCode);        
    }
}

