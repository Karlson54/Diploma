using TimeTracker.Data.Context;
using TimeTracker.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.API.Extensions;

public static class SeedExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<TimeTrackerDbContext>();

            await context.Database.MigrateAsync();

            var seeders = new ISeeder[]
            {
                new RoleSeed(context, services.GetRequiredService<ILogger<RoleSeed>>()),
                new DictionarySeed(context, services.GetRequiredService<ILogger<DictionarySeed>>()),
                new UserSeed(context, services.GetRequiredService<ILogger<UserSeed>>())
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync();
            }

            logger.LogInformation("Seed даних успішно завершено");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Помилка під час виконання Seed даних");
            throw;
        }
    }
}