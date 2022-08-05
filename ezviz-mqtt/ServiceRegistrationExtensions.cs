using ezviz.net.util;
using ezviz.netmqtt;
using ezviz_mqtt.config;
using ezviz_mqtt.health;
using ezviz_mqtt.util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ezviz_mqtt
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddMqttPublisher<T>(this IServiceCollection services, IConfiguration configuration) where T: BackgroundService
        {
            services.AddHostedService<T>();
            services.AddOptions();

#pragma warning disable IL2026
            services.Configure<EzvizOptions>(configuration.GetSection("ezviz"));
            services.Configure<MqttOptions>(configuration.GetSection("mqtt"));
            services.Configure<JsonOptions>(configuration.GetSection("json"));
            services.Configure<PollingOptions>(configuration.GetSection("polling"));
#pragma warning restore IL2026
            services.AddSingleton<IMqttPublisher, MqttPublisher>();
            
            services.AddHealthCheck(configuration);
            services.AddEzvizService(configuration);

            services.AddSingleton<IRequestResponseLogger, FileRequestResponseLogger>();
            return services;
        }
    }
}
