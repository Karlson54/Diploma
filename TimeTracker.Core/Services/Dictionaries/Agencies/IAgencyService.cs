using TimeTracker.Core.DTOs.Dictionaries.Agencies;

namespace TimeTracker.Core.Services.Dictionaries.Agencies;

public interface IAgencyService : IDictionaryService<AgencyDto, CreateAgencyDto, UpdateAgencyDto>
{
    // Додаткові методи специфічні для Agency
    Task<IEnumerable<AgencyDto>> GetByCountryAsync(string country);
    Task<AgencyDto?> GetWithUsersCountAsync(long id);
    Task<int> GetUsersCountAsync(long id);
    Task<bool> HasActiveUsersAsync(long id);
}