using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries;

public class DictionaryService<TEntity, TDto, TCreateDto, TUpdateDto> 
    : IDictionaryService<TDto, TCreateDto, TUpdateDto>
    where TEntity : DictionaryEntity, new()
    where TDto : DictionaryDto
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly IDictionaryRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly ILogger _logger;
    protected readonly IAuditService _auditService;
    protected readonly string _entityName;

    public DictionaryService(
        IDictionaryRepository<TEntity> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger logger,
        IAuditService auditService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _auditService = auditService;
        _entityName = typeof(TEntity).Name;
    }

    public virtual async Task<TDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var entity = await _repository.GetByNameAsync(name);
        return entity == null ? null : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<IEnumerable<TDto>> GetActiveAsync()
    {
        var entities = await _repository.GetActiveAsync();
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }

    public virtual async Task<TDto> CreateAsync(
        TCreateDto dto,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var entity = _mapper.Map<TEntity>(dto);

        // Перевірка унікальності назви - логуємо в консоль
        if (await _repository.IsNameExistsAsync(entity.Name))
        {
            _logger.LogWarning(
                "Спроба створення {EntityName} з існуючою назвою: {Name}",
                _entityName, entity.Name);
            throw new InvalidOperationException(
                $"{_entityName} з назвою '{entity.Name}' вже існує");
        }

        entity.IsActive = true;
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} створено успішно. Id: {Id}, Name: {Name}",
            _entityName, entity.Id, entity.Name);

        //АУДИТ В БД - бізнес-логіка
        await _auditService.LogDictionaryCreatedAsync(
            dictionaryType: _entityName,
            dictionaryId: entity.Id,
            dictionaryName: entity.Name,
            newValues: entity,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> UpdateAsync(
        long id,
        TUpdateDto dto,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            _logger.LogWarning(
                "Спроба оновлення неіснуючого {EntityName} з ID {Id}",
                _entityName, id);
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        // Зберігаємо старі значення для аудиту
        var oldValues = new
        {
            entity.Id,
            entity.Name,
            entity.IsActive
        };

        // Оновлюємо entity
        _mapper.Map(dto, entity);

        // Перевірка унікальності назви при зміні - логуємо в консоль
        var nameChanged = !string.Equals(oldValues.Name, entity.Name, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && await _repository.IsNameExistsAsync(entity.Name, id))
        {
            _logger.LogWarning(
                "Спроба оновлення {EntityName} {Id} з існуючою назвою: {Name}",
                _entityName, id, entity.Name);
            throw new InvalidOperationException(
                $"{_entityName} з назвою '{entity.Name}' вже існує");
        }

         _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} оновлено успішно. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);

        //АУДИТ В БД - бізнес-логіка
        await _auditService.LogDictionaryUpdatedAsync(
            dictionaryType: _entityName,
            dictionaryId: entity.Id,
            dictionaryName: entity.Name,
            oldValues: oldValues,
            newValues: new
            {
                entity.Id,
                entity.Name,
                entity.IsActive
            },
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task DeleteAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        // Перевірка можливості видалення - логуємо в консоль
        if (!await CanBeDeletedAsync(id))
        {
            _logger.LogWarning(
                "Спроба видалення {EntityName} '{Name}' (ID: {Id}), який використовується",
                _entityName, entity.Name, id);
            throw new InvalidOperationException(
                $"Неможливо видалити {_entityName} '{entity.Name}', оскільки він використовується");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            entity.Id,
            entity.Name,
            entity.IsActive
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} видалено. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);

        //АУДИТ В БД - бізнес-логіка (Delete)
        await _auditService.LogDeleteAsync(
            entityName: _entityName,
            entityId: id,
            oldValues: oldValues,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public virtual async Task ActivateAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        if (entity.IsActive)
        {
            _logger.LogWarning(
                "Спроба активації вже активного {EntityName} '{Name}' (ID: {Id})",
                _entityName, entity.Name, id);
            throw new InvalidOperationException($"{_entityName} вже активований");
        }

        await _repository.ActivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} активовано. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);

        //АУДИТ В БД - бізнес-логіка
        await _auditService.LogDictionaryActivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: entity.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public virtual async Task DeactivateAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        if (!entity.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого {EntityName} '{Name}' (ID: {Id})",
                _entityName, entity.Name, id);
            throw new InvalidOperationException($"{_entityName} вже деактивований");
        }

        // Перевірка можливості деактивації - логуємо в консоль
        if (!await CanBeDeactivatedAsync(id))
        {
            _logger.LogWarning(
                "Неможливо деактивувати {EntityName} '{Name}' (ID: {Id}) - використовується в активних записах",
                _entityName, entity.Name, id);
            throw new InvalidOperationException(
                $"Неможливо деактивувати {_entityName} '{entity.Name}', " +
                "оскільки він використовується в активних записах");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} деактивовано. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);

        //АУДИТ В БД - бізнес-логіка
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: entity.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public virtual async Task<bool> IsNameExistsAsync(string name, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (excludeId.HasValue)
            return await _repository.IsNameExistsAsync(name, excludeId.Value);

        return await _repository.IsNameExistsAsync(name);
    }

    public virtual async Task<bool> CanBeDeactivatedAsync(long id)
    {
        return await _repository.CanBeDeactivatedAsync(id);
    }

    // Додаємо віртуальний метод для перевірки можливості видалення
    protected virtual async Task<bool> CanBeDeletedAsync(long id)
    {
        // За замовчуванням можна видалити
        // Кожен конкретний сервіс може перевизначити цю логіку
        return await Task.FromResult(true);
    }

    public virtual async Task<int> GetActiveCountAsync()
    {
        return await _repository.GetActiveCountAsync();
    }

    public virtual async Task<int> GetTotalCountAsync()
    {
        return await _repository.CountAsync();
    }

    public virtual async Task<(IEnumerable<TDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // використовуємо GetQueryable() замість GetPagedAsync з параметрами
        var query = _repository.GetQueryable();

        // Фільтр за пошуковим терміном
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(term));
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

        var dtos = _mapper.Map<IEnumerable<TDto>>(entities);

        return (dtos, totalCount);
    }
}