using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Agencies;
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
        ILogger<AgencyService> logger)
        : base(repository, unitOfWork, mapper, logger)
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
            .OrderBy(a => a.Name)
            .AsNoTracking()
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
        // Перевіряємо чи є користувачі
        var hasUsers = await _unitOfWork.Users
            .GetQueryable()
            .AnyAsync(u => u.AgencyId == id);

        if (hasUsers)
            return false;

        // Перевіряємо чи є TimeEntries
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.AgencyId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        // Можна деактивувати тільки якщо немає активних користувачів
        var hasActiveUsers = await HasActiveUsersAsync(id);
        
        if (hasActiveUsers)
        {
            _logger.LogWarning(
                "Неможливо деактивувати Agency {Id}, оскільки є активні користувачі",
                id);
            return false;
        }

        // І немає активних TimeEntries
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.AgencyId == id);

        return !hasActiveTimeEntries;
    }

    public override async Task DeactivateAsync(long id)
    {
        var agency = await _repository.GetByIdAsync(id);
        if (agency == null)
        {
            throw new KeyNotFoundException($"Agency з ID {id} не знайдено");
        }

        if (!agency.IsActive)
        {
            throw new InvalidOperationException("Agency вже деактивований");
        }

        // Додаткова перевірка активних користувачів
        if (await HasActiveUsersAsync(id))
        {
            var usersCount = await _unitOfWork.Users
                .GetQueryable()
                .CountAsync(u => u.AgencyId == id && u.IsActive);

            throw new InvalidOperationException(
                $"Неможливо деактивувати Agency '{agency.Name}', " +
                $"оскільки є {usersCount} активних користувачів. " +
                "Спочатку деактивуйте або перемістіть всіх користувачів.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Agency деактивовано. Id: {Id}, Name: {Name}, Country: {Country}",
            id, agency.Name, agency.Country);
    }

    public override async Task<(IEnumerable<AgencyDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _repository.GetQueryable();

        // Фільтр за пошуковим терміном (шукаємо і в назві і в країні)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(term) || 
                e.Country.ToLower().Contains(term));
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

        var dtos = _mapper.Map<List<AgencyDto>>(entities);

        // Додаємо кількість користувачів
        foreach (var dto in dtos)
        {
            dto.UsersCount = await GetUsersCountAsync(dto.Id);
        }

        return (dtos, totalCount);
    }
}