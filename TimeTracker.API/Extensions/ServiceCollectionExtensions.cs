using TimeTracker.Core.Services.Auth;
using TimeTracker.Core.Services.Dictionaries.Agencies;
using TimeTracker.Core.Services.Dictionaries.Clients;
using TimeTracker.Core.Services.Dictionaries.ContractingAgencies;
using TimeTracker.Core.Services.Dictionaries.JobTypes;
using TimeTracker.Core.Services.Dictionaries.Markets;
using TimeTracker.Core.Services.Dictionaries.Media;
using TimeTracker.Core.Services.Dictionaries.ProjectBrands;
using TimeTracker.Core.Services.RoleManagement;
using TimeTracker.Core.Services.UserManagement;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Data.Repositories.Dictionaries;
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

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Specialized Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        
        // Dictionary Repositories
        services.AddScoped<IDictionaryRepository<Agency>, DictionaryRepository<Agency>>();
        services.AddScoped<IDictionaryRepository<Market>, DictionaryRepository<Market>>();
        services.AddScoped<IDictionaryRepository<ContractingAgency>, DictionaryRepository<ContractingAgency>>();
        services.AddScoped<IDictionaryRepository<Client>, DictionaryRepository<Client>>();
        services.AddScoped<IDictionaryRepository<Data.Entities.Media>, DictionaryRepository<Data.Entities.Media>>();
        services.AddScoped<IDictionaryRepository<JobType>, DictionaryRepository<JobType>>();
        services.AddScoped<IDictionaryRepository<ProjectBrand>, DictionaryRepository<ProjectBrand>>();

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // User & Role Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        
        // Dictionary Services
        services.AddScoped<IAgencyService, AgencyService>();
        services.AddScoped<IMarketService, MarketService>();
        services.AddScoped<IContractingAgencyService, ContractingAgencyService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IJobTypeService, JobTypeService>();
        services.AddScoped<IProjectBrandService, ProjectBrandService>();

        return services;
    }
}