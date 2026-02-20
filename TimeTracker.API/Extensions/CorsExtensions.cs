namespace TimeTracker.API.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "TimeTrackerCors";

    public static IServiceCollection AddTimeTrackerCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseTimeTrackerCors(this IApplicationBuilder app)
    {
        app.UseCors(PolicyName);
        return app;
    }
}