using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TinyHealthCheck;

namespace ezviz_mqtt.health;

public static class HealthCheckRegistration
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<MqttServiceState>();

        services.AddBasicTinyHealthCheckWithUptime(config =>
        {
            var healthPortConfig = configuration["health:uptimeport"];

            config.Port = healthPortConfig == null ? 8081 : int.Parse(healthPortConfig);
            config.UrlPath = "/uptime";
            return config;
        });
        services.AddCustomTinyHealthCheck<MqttHealthCheck>(config =>
        {
            var healthPortConfig = configuration["health:statusport"];

            config.Port = healthPortConfig == null ? 8082 : int.Parse(healthPortConfig);
            config.UrlPath = "/status";            
            return config;
        });

        return services;
    }
}

