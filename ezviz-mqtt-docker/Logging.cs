internal static class Logging
{
    internal static IServiceCollection AddSystemdLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddSystemdConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "HH:mm:ss";
            });
        });
        return services;
    }
}