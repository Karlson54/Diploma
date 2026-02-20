using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace TimeTracker.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string GlobalPolicy = "global";

    public static IServiceCollection AddTimeTrackerRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Глобальная политика по умолчанию для всех endpoints
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 300,
                        QueueLimit = 0
                    }));

            // Строгая политика для auth endpoints
            options.AddFixedWindowLimiter(AuthPolicy, limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 10;
                limiter.QueueLimit = 0;
                limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}