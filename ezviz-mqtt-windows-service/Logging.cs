internal static class Logging
{
    internal static IServiceCollection AddWindowsLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddEventLog(options =>
            {
                options.SourceName = "ezviz-mqtt";
                options.LogName = "ezviz-mqtt";
            });
        });
        return services;
    }
}