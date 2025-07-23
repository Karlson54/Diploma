using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Data.Configurations;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Context;

public class TimeTrackerDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public TimeTrackerDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public DbSet<Agency> Agencies { get; set; }
    public DbSet<Market> Markets { get; set; }
    public DbSet<ContractingAgency> ContractingAgencies { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<JobType> JobTypes { get; set; }
    public DbSet<ProjectBrand> ProjectBrands { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimeTrackerDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
    {
            optionbuilder
                .UseSqlServer(_configuration.GetConnectionString("TimeTracker"))
                .EnableServiceProviderCaching()
                .EnableDetailedErrors();
    }
}