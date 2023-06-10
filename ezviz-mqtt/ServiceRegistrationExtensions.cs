using ezviz.net.util;
using ezviz.netmqtt;
using ezviz_mqtt.commands;
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
        public static IServiceCollection AddMqttWorker<T>(this IServiceCollection services, IConfiguration configuration) where T: BackgroundService
        {
            services.AddHostedService<T>();
            services.AddOptions();

#pragma warning disable IL2026
            var mqttOptions = configuration.GetSection("mqtt");
            services.Configure<EzvizOptions>(configuration.GetSection("ezviz"));
            services.Configure<MqttOptions>(mqttOptions);
            services.Configure<JsonOptions>(configuration.GetSection("json"));
            services.Configure<PollingOptions>(configuration.GetSection("polling"));
#pragma warning restore IL2026
            services.AddSingleton<IMqttWorker, MqttWorker>();
            
            services.AddHealthCheck(configuration);
            services.AddEzvizService(configuration);

            services.AddSingleton<IRequestResponseLogger, FileRequestResponseLogger>();
            services.AddSingleton<IPushNotificationLogger, PushNotificationLogger>();

            var boundMqtt = new MqttOptions();
            mqttOptions.Bind(boundMqtt);            
            services.AddSingleton(new TopicExtensions(boundMqtt.Topics, StringComparer.OrdinalIgnoreCase));

            services.AddSingleton<IMqttHandler, MqttHandler>();
            services.AddSingleton<IStateCommandFactory, StateCommandFactory>();
            return services;
        }
    }
}
