using ezviz_mqtt.config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ezviz_mqtt
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddMqttPublisher(this IServiceCollection services, IConfiguration configuration)
        {
#pragma warning disable IL2026
            services.Configure<EzvizOptions>(configuration.GetSection("ezviz"));
            services.Configure<MqttOptions>(configuration.GetSection("mqtt"));
            services.Configure<JsonOptions>(configuration.GetSection("json"));
            services.Configure<PollingOptions>(configuration.GetSection("polling"));
#pragma warning restore IL2026
            services.AddSingleton<IMqttPublisher, MqttPublisher>();
            return services;
        }
    }
}
