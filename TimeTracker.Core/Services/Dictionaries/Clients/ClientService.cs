using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Clients;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.Clients;

public class ClientService : DictionaryService<Client, ClientDto, CreateClientDto, UpdateClientDto>, IClientService
{
    public ClientService(
        IDictionaryRepository<Client> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ClientService> logger)
        : base(repository, unitOfWork, mapper, logger)
    {
    }

    public override async Task<ClientDto> CreateAsync(CreateClientDto dto)
    {
        // Перевірка унікальності email якщо він вказаний
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (await IsEmailExistsAsync(dto.Email))
            {
                _logger.LogWarning(
                    "Спроба створення Client з існуючим email: {Email}",
                    dto.Email);
                throw new InvalidOperationException(
                    $"Client з email '{dto.Email}' вже існує");
            }
        }

        return await base.CreateAsync(dto);
    }

    public override async Task<ClientDto> UpdateAsync(long id, UpdateClientDto dto)
    {
        var client = await _repository.GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client з ID {id} не знайдено");
        }

        // Перевірка унікальності email якщо він змінюється
        if (!string.IsNullOrWhiteSpace(dto.Email) && 
            !string.Equals(client.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await IsEmailExistsAsync(dto.Email, id))
            {
                _logger.LogWarning(
                    "Спроба оновлення Client {Id} з існуючим email: {Email}",
                    id, dto.Email);
                throw new InvalidOperationException(
                    $"Client з email '{dto.Email}' вже існує");
            }
        }

        return await base.UpdateAsync(id, dto);
    }

    public async Task<ClientDto?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var client = await _repository
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Email != null && 
                                     c.Email.ToLower() == email.ToLower());

        return client == null ? null : _mapper.Map<ClientDto>(client);
    }

    public async Task<IEnumerable<ClientDto>> SearchByEmailOrPhoneAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ClientDto>();

        var term = searchTerm.ToLower();

        var clients = await _repository
            .GetQueryable()
            .Where(c => (c.Email != null && c.Email.ToLower().Contains(term)) ||
                       (c.Phone != null && c.Phone.ToLower().Contains(term)) ||
                       c.Name.ToLower().Contains(term))
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<ClientDto>>(clients);
    }

    public async Task<bool> IsEmailExistsAsync(string email, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _repository.GetQueryable()
            .Where(c => c.Email != null && c.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetTimeEntriesCountAsync(long id)
    {
        return await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.ClientId == id);
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        // Перевіряємо чи є TimeEntries
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ClientId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        // Можна деактивувати тільки якщо немає активних TimeEntries
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ClientId == id);

        return !hasActiveTimeEntries;
    }

    public override async Task<(IEnumerable<ClientDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _repository.GetQueryable();

        // Фільтр за пошуковим терміном (шукаємо в назві, email та phone)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(term) ||
                (e.Email != null && e.Email.ToLower().Contains(term)) ||
                (e.Phone != null && e.Phone.ToLower().Contains(term)));
        }

        // Фільтр за активністю
        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var entities = await query
            .OrderBy(e => e.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<IEnumerable<ClientDto>>(entities);

        return (dtos, totalCount);
    }
}