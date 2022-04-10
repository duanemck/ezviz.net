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

    public Task<IHealthCheckResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(serviceState);
        serviceState.MostRecentError = null;
        serviceState.MostRecentErrorTime = null;
        return Task.FromResult<IHealthCheckResult>(new HealthCheckResult(json, serviceState.StatusCode));        
    }
}

