using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Clients;
using TimeTracker.Core.Services.Audit;
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
        ILogger<ClientService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    public override async Task<ClientDto> CreateAsync(
        CreateClientDto dto,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        // Перевірка унікальності email якщо він вказаний - логуємо в консоль
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

        return await base.CreateAsync(dto, userId, userName, ipAddress, userAgent);
    }

    public override async Task<ClientDto> UpdateAsync(
        long id,
        UpdateClientDto dto,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var client = await _repository.GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client з ID {id} не знайдено");
        }

        // Перевірка унікальності email якщо він змінюється - логуємо в консоль
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

        return await base.UpdateAsync(id, dto, userId, userName, ipAddress, userAgent);
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

    //DeleteAsync для детальних помилок
    public override async Task DeleteAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var client = await _repository.GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await GetTimeEntriesCountAsync(id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення Client '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                client.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити Client '{client.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            client.Id,
            client.Name,
            client.IsActive,
            client.Email,
            client.Phone
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ В БД
        await _auditService.LogDeleteAsync(
            entityName: _entityName,
            entityId: id,
            oldValues: oldValues,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    //DeactivateAsync для детальних помилок
    public override async Task DeactivateAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var client = await _repository.GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException($"Client з ID {id} не знайдено");
        }

        if (!client.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого Client '{Name}' (ID: {Id})",
                client.Name, id);
            throw new InvalidOperationException("Client вже деактивований");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: client.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
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