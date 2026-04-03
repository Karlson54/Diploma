using TimeTracker.Core.Services.Auth;
using TimeTracker.Core.Services.Dictionaries.Agencies;
using TimeTracker.Core.Services.Dictionaries.Clients;
using TimeTracker.Core.Services.Dictionaries.ContractingAgencies;
using TimeTracker.Core.Services.Dictionaries.JobTypes;
using TimeTracker.Core.Services.Dictionaries.Markets;
using TimeTracker.Core.Services.Dictionaries.Media;
using TimeTracker.Core.Services.Reporting;
using TimeTracker.Core.Services.RoleManagement;
using TimeTracker.Core.Services.TimeTracking;
using TimeTracker.Core.Services.UserManagement;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Repositories.Audit;

namespace TimeTracker.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimeTrackerServices(this IServiceCollection services)
    {
        //Memory Cache для ReportService
        services.AddMemoryCache();

        // AutoMapper
        services.AddAutoMapper(typeof(Core.Mappings.UserMappingProfile).Assembly);

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Specialized Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Dictionary Repositories
        services.AddScoped<IDictionaryRepository<Agency>, DictionaryRepository<Agency>>();
        services.AddScoped<IDictionaryRepository<Market>, DictionaryRepository<Market>>();
        services.AddScoped<IDictionaryRepository<ContractingAgency>, DictionaryRepository<ContractingAgency>>();
        services.AddScoped<IDictionaryRepository<Client>, DictionaryRepository<Client>>();
        services.AddScoped<IDictionaryRepository<Media>, DictionaryRepository<Media>>();
        services.AddScoped<IDictionaryRepository<JobType>, DictionaryRepository<JobType>>();

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

        // TimeEntry Services
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<ITimeValidationService, TimeValidationService>();

        // Report Services
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExportService, ExportService>();

        // Audit Service
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}