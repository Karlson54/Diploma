using TimeTracker.Core.Services.Auth;
using TimeTracker.Core.Services.RoleManagement;
using TimeTracker.Core.Services.UserManagement;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimeTrackerServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(TimeTracker.Core.Mappings.UserMappingProfile).Assembly);

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}