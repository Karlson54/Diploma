using TimeTracker.Core.DTOs.Dictionaries.Clients;

namespace TimeTracker.Core.Services.Dictionaries.Clients;

public interface IClientService : IDictionaryService<ClientDto, CreateClientDto, UpdateClientDto>
{
    // Додаткові методи специфічні для Client
    Task<ClientDto?> GetByEmailAsync(string email);
    Task<IEnumerable<ClientDto>> SearchByEmailOrPhoneAsync(string searchTerm);
    Task<bool> IsEmailExistsAsync(string email, long? excludeId = null);
    Task<int> GetTimeEntriesCountAsync(long id);
}