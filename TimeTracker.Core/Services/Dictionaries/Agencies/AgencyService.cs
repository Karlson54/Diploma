using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Agencies;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.Agencies;

public class AgencyService : DictionaryService<Agency, AgencyDto, CreateAgencyDto, UpdateAgencyDto>, IAgencyService
{
    public AgencyService(
        IDictionaryRepository<Agency> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AgencyService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    public override async Task<AgencyDto?> GetByIdAsync(long id)
    {
        var agency = await _repository.GetByIdAsync(id);
        if (agency == null)
            return null;

        var dto = _mapper.Map<AgencyDto>(agency);
        
        // Додаємо кількість користувачів
        dto.UsersCount = await GetUsersCountAsync(id);
        
        return dto;
    }

    public override async Task<IEnumerable<AgencyDto>> GetAllAsync()
    {
        var agencies = await _repository.GetAllAsync();
        var dtos = _mapper.Map<List<AgencyDto>>(agencies);

        // Додаємо кількість користувачів для кожного agency
        foreach (var dto in dtos)
        {
            dto.UsersCount = await GetUsersCountAsync(dto.Id);
        }

        return dtos;
    }

    public async Task<IEnumerable<AgencyDto>> GetByCountryAsync(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Enumerable.Empty<AgencyDto>();

        var agencies = await _repository
            .GetQueryable()
            .Where(a => a.Country.ToLower() == country.ToLower())
            .ToListAsync();

        var dtos = _mapper.Map<List<AgencyDto>>(agencies);

        foreach (var dto in dtos)
        {
            dto.UsersCount = await GetUsersCountAsync(dto.Id);
        }

        return dtos;
    }

    public async Task<AgencyDto?> GetWithUsersCountAsync(long id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<int> GetUsersCountAsync(long id)
    {
        return await _unitOfWork.Users
            .GetQueryable()
            .CountAsync(u => u.AgencyId == id);
    }

    public async Task<bool> HasActiveUsersAsync(long id)
    {
        return await _unitOfWork.Users
            .GetQueryable()
            .AnyAsync(u => u.AgencyId == id && u.IsActive);
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        // Не можна видалити Agency якщо є користувачі
        var hasUsers = await _unitOfWork.Users
            .GetQueryable()
            .AnyAsync(u => u.AgencyId == id);

        return !hasUsers;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        // Можна деактивувати тільки якщо немає активних користувачів
        var hasActiveUsers = await HasActiveUsersAsync(id);
        return !hasActiveUsers;
    }

    //DeactivateAsync для детальних помилок
    public override async Task DeactivateAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var agency = await _repository.GetByIdAsync(id);
        if (agency == null)
        {
            throw new KeyNotFoundException($"Agency з ID {id} не знайдено");
        }

        if (!agency.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого Agency '{Name}' (ID: {Id})",
                agency.Name, id);
            throw new InvalidOperationException("Agency вже деактивоване");
        }

        // Перевірка активних користувачів
        var activeUsersCount = await _unitOfWork.Users
            .GetQueryable()
            .CountAsync(u => u.AgencyId == id && u.IsActive);

        if (activeUsersCount > 0)
        {
            _logger.LogWarning(
                "Неможливо деактивувати Agency '{Name}' (ID: {Id}) - є {Count} активних користувачів",
                agency.Name, id, activeUsersCount);
            
            throw new InvalidOperationException(
                $"Неможливо деактивувати Agency '{agency.Name}', " +
                $"оскільки є {activeUsersCount} активних користувачів. " +
                "Спочатку деактивуйте або перемістіть всіх користувачів.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Agency деактивовано. Id: {Id}, Name: {Name}",
            id, agency.Name);

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: agency.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    //DeleteAsync для детальних помилок
    public override async Task DeleteAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var agency = await _repository.GetByIdAsync(id);
        if (agency == null)
        {
            throw new KeyNotFoundException($"Agency з ID {id} не знайдено");
        }

        // Перевірка наявності користувачів
        var usersCount = await GetUsersCountAsync(id);
        
        if (usersCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення Agency '{Name}' (ID: {Id}), який має {Count} користувачів",
                agency.Name, id, usersCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити Agency '{agency.Name}', " +
                $"оскільки до нього прив'язано {usersCount} користувачів. " +
                "Спочатку перемістіть або видаліть всіх користувачів.");
        }

        // Зберігаємо дані для аудиту - ТІЛЬКИ реальні поля Agency
        var oldValues = new
        {
            agency.Id,
            agency.Name,
            agency.IsActive,
            agency.Country
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Agency видалено. Id: {Id}, Name: {Name}",
            id, agency.Name);

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
}