using TimeTracker.Core.DTOs.Dictionaries.Agencies;

namespace TimeTracker.Core.Services.Dictionaries.Agencies;

public interface IAgencyService : IDictionaryService<AgencyDto, CreateAgencyDto, UpdateAgencyDto>
{
    Task<AgencyDto?> GetWithUsersCountAsync(long id);
    Task<int> GetUsersCountAsync(long id);
    Task<bool> HasActiveUsersAsync(long id);
}