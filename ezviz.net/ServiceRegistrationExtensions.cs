using ezviz.net;
using ezviz.net.util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ezviz.netmqtt
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddEzvizService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEzvizClient, EzvizClient>();
            services.AddSingleton<IRequestResponseLogger, DefaultRequestResponseLogger>();

            return services;
        }
    }
}
