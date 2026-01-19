using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;

namespace TimeTracker.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddTimeTrackerDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<TimeTrackerDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("TimeTracker"),
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(
                        typeof(TimeTrackerDbContext).Assembly.GetName().Name);
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

            // Детальные логи только в Development
            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                // options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        services.AddScoped<ITimeTrackerDbContext>(provider =>
            provider.GetRequiredService<TimeTrackerDbContext>());

        return services;
    }
}