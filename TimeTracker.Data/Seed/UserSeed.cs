using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Seed;

public class UserSeed : ISeeder
{
    private readonly TimeTrackerDbContext _context;
    private readonly ILogger<UserSeed> _logger;

    private const string SuperAdminLogin = "superAdmin";
    private const string SuperAdminEmail = "superAdmin@mediacom.ua";
    private const string SuperAdminPassword = "Admin122!";
    private const string SuperAdminAgency = "MediaCom";

    public UserSeed(TimeTrackerDbContext context, ILogger<UserSeed> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_context.Users.Any(u => u.Login == SuperAdminLogin))
        {
            _logger.LogInformation("Super Admin вже існує, пропускаємо");
            return;
        }

        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Name == SuperAdminAgency);

        if (agency == null)
        {
            _logger.LogError("Agency '{Agency}' не знайдено. UserSeed не виконано", SuperAdminAgency);
            return;
        }

        var superAdminRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        if (superAdminRole == null)
        {
            _logger.LogError("Роль 'SuperAdmin' не знайдено. UserSeed не виконано");
            return;
        }

        // Знаходимо або створюємо департамент за замовчуванням для agency
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.AgencyId == agency.Id);

        if (department == null)
        {
            department = new Department
            {
                Name = $"{agency.Name}Default",
                AgencyId = agency.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Department 'Default' створено для Agency '{Agency}'", SuperAdminAgency);
        }

        var superAdmin = new User
        {
            Name = "System Administrator",
            Login = SuperAdminLogin,
            Email = SuperAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(SuperAdminPassword),
            AgencyId = agency.Id,
            DepartmentId = department.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(superAdmin);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = superAdmin.Id,
            RoleId = superAdminRole.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "SuperAdmin користувач створено. Login: '{Login}', Agency: '{Agency}', Department: '{Department}'",
            SuperAdminLogin, SuperAdminAgency, department.Name);
    }
}