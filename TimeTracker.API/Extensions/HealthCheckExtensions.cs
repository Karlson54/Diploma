using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TimeTracker.Data.Context;

namespace TimeTracker.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddTimeTrackerHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<TimeTrackerDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "sql" });

        return services;
    }

    public static IEndpointRouteBuilder MapTimeTrackerHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        // Общий health check — приложение живо?
        endpoints.MapHealthChecks("/health");

        // Readiness — база доступна?
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db")
        });

        return endpoints;
    }
}