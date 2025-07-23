using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Context;

public interface ITimeTrackerDbContext
{
    DbSet<Agency> Agencies { get; }
    DbSet<Market> Markets { get; }
    DbSet<ContractingAgency> ContractingAgencies { get; }
    DbSet<Client> Clients { get; }
    DbSet<Media> Media { get; }
    DbSet<JobType> JobTypes { get; }
    DbSet<ProjectBrand> ProjectBrands { get; }
    DbSet<Role> Roles { get; }
    DbSet<User> Users { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<TimeEntry> TimeEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}