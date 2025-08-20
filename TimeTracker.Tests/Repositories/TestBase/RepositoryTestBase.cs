using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Tests.Repositories.TestBase;

public abstract class RepositoryTestBase : IDisposable
{
    protected TimeTrackerDbContext Context { get; }

    protected RepositoryTestBase()
    {
        // Каждый тест получает уникальную базу данных
        var options = new DbContextOptionsBuilder<TimeTrackerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new TimeTrackerDbContext(options);
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Агентства
        var agencies = new[]
        {
            new Agency { Id = 1, Name = "MediaCom Ukraine", Country = "Ukraine", IsActive = true },
            new Agency { Id = 2, Name = "Mindshare", Country = "Ukraine", IsActive = true },
            new Agency { Id = 3, Name = "Inactive Agency", Country = "Ukraine", IsActive = false }
        };

        // Роли
        var roles = new[]
        {
            new Role { Id = 1, Name = "Admin", Description = "Administrator", IsActive = true },
            new Role { Id = 2, Name = "Manager", Description = "Manager", IsActive = true },
            new Role { Id = 3, Name = "Employee", Description = "Employee", IsActive = true },
            new Role { Id = 4, Name = "Inactive Role", Description = "Inactive", IsActive = false }
        };

        // Пользователи
        var users = new[]
        {
            new User 
            { 
                Id = 1, 
                Name = "John Doe", 
                Email = "john@example.com", 
                Login = "john.doe",
                PasswordHash = "hash1",
                AgencyId = 1,
                IsActive = true 
            },
            new User 
            { 
                Id = 2, 
                Name = "Jane Smith", 
                Email = "jane@example.com", 
                Login = "jane.smith",
                PasswordHash = "hash2",
                AgencyId = 1,
                IsActive = true 
            },
            new User 
            { 
                Id = 3, 
                Name = "Bob Johnson", 
                Email = "bob@example.com", 
                Login = "bob.johnson",
                PasswordHash = "hash3",
                AgencyId = 2,
                IsActive = false 
            }
        };

        // Связи ролей и пользователей
        var userRoles = new[]
        {
            new UserRole { UserId = 1, RoleId = 1 }, // John - Admin
            new UserRole { UserId = 1, RoleId = 2 }, // John - Manager
            new UserRole { UserId = 2, RoleId = 3 }, // Jane - Employee
            new UserRole { UserId = 3, RoleId = 3 }  // Bob - Employee
        };

        // Клиенты
        var clients = new[]
        {
            new Client { Id = 1, Name = "Procter & Gamble", Email = "contact@pg.com", IsActive = true },
            new Client { Id = 2, Name = "Nestlé", Email = "info@nestle.com", IsActive = true },
            new Client { Id = 3, Name = "Inactive Client", IsActive = false }
        };

        // Проекты
        var projects = new[]
        {
            new ProjectBrand { Id = 1, Name = "Pampers Campaign", IsActive = true },
            new ProjectBrand { Id = 2, Name = "Nescafe Launch", IsActive = true },
            new ProjectBrand { Id = 3, Name = "Inactive Project", IsActive = false }
        };

        // Типы работ
        var jobTypes = new[]
        {
            new JobType { Id = 1, Name = "Strategy Planning", IsActive = true },
            new JobType { Id = 2, Name = "Media Planning", IsActive = true },
            new JobType { Id = 3, Name = "Inactive Job", IsActive = false }
        };

        // Рынки
        var markets = new[]
        {
            new Market { Id = 1, Name = "Ukraine", IsActive = true },
            new Market { Id = 2, Name = "Europe", IsActive = true }
        };

        // Медиа
        var media = new[]
        {
            new Media { Id = 1, Name = "Digital", IsActive = true },
            new Media { Id = 2, Name = "TV", IsActive = true }
        };

        // Подрядчики
        var contractingAgencies = new[]
        {
            new ContractingAgency { Id = 1, Name = "GroupM", IsActive = true },
            new ContractingAgency { Id = 2, Name = "WPP", IsActive = true }
        };

        // Записи времени
        var timeEntries = new[]
        {
            new TimeEntry
            {
                Id = 1,
                UserId = 1,
                EntryDate = DateTime.Today,
                AgencyId = 1,
                MarketId = 1,
                ContractingAgencyId = 1,
                ClientId = 1,
                ProjectBrandId = 1,
                MediaId = 1,
                JobTypeId = 1,
                HoursMilliseconds = 8 * 3600000, // 8 часов
                Comments = "Strategy work"
            },
            new TimeEntry
            {
                Id = 2,
                UserId = 2,
                EntryDate = DateTime.Today,
                AgencyId = 1,
                MarketId = 1,
                ContractingAgencyId = 1,
                ClientId = 2,
                ProjectBrandId = 2,
                MediaId = 2,
                JobTypeId = 2,
                HoursMilliseconds = 6 * 3600000, // 6 часов
                Comments = "Media planning"
            },
            new TimeEntry
            {
                Id = 3,
                UserId = 1,
                EntryDate = DateTime.Today.AddDays(-1),
                AgencyId = 1,
                MarketId = 1,
                ContractingAgencyId = 1,
                ClientId = 1,
                ProjectBrandId = 1,
                MediaId = 1,
                JobTypeId = 1,
                HoursMilliseconds = 4 * 3600000, // 4 часа
                Comments = "Yesterday work"
            }
        };

        Context.Agencies.AddRange(agencies);
        Context.Roles.AddRange(roles);
        Context.Users.AddRange(users);
        Context.UserRoles.AddRange(userRoles);
        Context.Clients.AddRange(clients);
        Context.ProjectBrands.AddRange(projects);
        Context.JobTypes.AddRange(jobTypes);
        Context.Markets.AddRange(markets);
        Context.Media.AddRange(media);
        Context.ContractingAgencies.AddRange(contractingAgencies);
        Context.TimeEntries.AddRange(timeEntries);

        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}