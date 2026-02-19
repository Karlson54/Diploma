using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Seed;

public class UserSeed : ISeeder
{
    private readonly TimeTrackerDbContext _context;
    private readonly ILogger<UserSeed> _logger;

    private const string AdminLogin = "admin";
    private const string AdminEmail = "admin@mediacom.ua";
    private const string AdminPassword = "Admin122!";
    private const string AdminAgency = "MediaCom";

    public UserSeed(TimeTrackerDbContext context, ILogger<UserSeed> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_context.Users.Any(u => u.Login == AdminLogin))
        {
            _logger.LogInformation("Admin вже існує, пропускаємо");
            return;
        }

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Name == AdminAgency);

        if (agency == null)
        {
            _logger.LogError("Agency '{Agency}' не знайдено. UserSeed не виконано", AdminAgency);
            return;
        }

        var adminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole == null)
        {
            _logger.LogError("Роль 'Admin' не знайдено. UserSeed не виконано");
            return;
        }

        var admin = new User
        {
            Name = "System Administrator",
            Login = AdminLogin,
            Email = AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword),
            AgencyId = agency.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(admin);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = admin.Id,
            RoleId = adminRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin користувач створено. Login: '{Login}', Agency: '{Agency}'",
            AdminLogin, AdminAgency);
    }
}