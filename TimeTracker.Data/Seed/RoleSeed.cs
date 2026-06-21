using Microsoft.Extensions.Logging;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Seed;

public class RoleSeed : ISeeder
{
    private readonly TimeTrackerDbContext _context;
    private readonly ILogger<RoleSeed> _logger;

    public RoleSeed(TimeTrackerDbContext context, ILogger<RoleSeed> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var roles = new[]
        {
            new Role
            {
                Name = "SuperAdmin", Description = "Супер адміністратор з повним доступом до системи", IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Admin", Description = "Адміністратор з доступом до дозволених агенцій та відділів",
                IsActive = true, CreatedAt = DateTime.UtcNow
            },
            new Role
            {
                Name = "Employee", Description = "Облік робочого часу", IsActive = true, CreatedAt = DateTime.UtcNow
            }
        };

        var created = false;

        foreach (var role in roles)
        {
            if (!_context.Roles.Any(r => r.Name == role.Name))
            {
                await _context.Roles.AddAsync(role);
                created = true;
                _logger.LogInformation("Роль '{RoleName}' створена", role.Name);
            }
        }

        if (created)
            await _context.SaveChangesAsync();
    }
}