using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries;
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
    protected readonly string _entityName;

    public DictionaryService(
        IDictionaryRepository<TEntity> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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

    public virtual async Task<TDto> CreateAsync(TCreateDto dto)
    {
        var entity = _mapper.Map<TEntity>(dto);

        // Перевірка унікальності назви
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

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> UpdateAsync(long id, TUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        // Зберігаємо стару назву для логування
        var oldName = entity.Name;

        // Мапимо нові дані
        _mapper.Map(dto, entity);

        // Перевірка унікальності нової назви
        if (await _repository.IsNameExistsAsync(entity.Name, id))
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
            "{EntityName} оновлено. Id: {Id}, OldName: {OldName}, NewName: {NewName}",
            _entityName, entity.Id, oldName, entity.Name);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task DeleteAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        // Перевірка чи можна видалити
        if (!await CanBeDeletedAsync(id))
        {
            throw new InvalidOperationException(
                $"Неможливо видалити {_entityName} '{entity.Name}', " +
                "оскільки він використовується в інших записах");
        }

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} видалено. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);
    }

    public virtual async Task ActivateAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        if (entity.IsActive)
        {
            throw new InvalidOperationException($"{_entityName} вже активний");
        }

        await _repository.ActivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} активовано. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);
    }

    public virtual async Task DeactivateAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"{_entityName} з ID {id} не знайдено");
        }

        if (!entity.IsActive)
        {
            throw new InvalidOperationException($"{_entityName} вже деактивований");
        }

        if (!await CanBeDeactivatedAsync(id))
        {
            throw new InvalidOperationException(
                $"Неможливо деактивувати {_entityName} '{entity.Name}', " +
                "оскільки він використовується в активних записах");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "{EntityName} деактивовано. Id: {Id}, Name: {Name}",
            _entityName, id, entity.Name);
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

    protected virtual async Task<bool> CanBeDeletedAsync(long id)
    {
        // За замовчуванням дозволяємо видаляти
        // Спеціалізовані сервіси переоприділять цей метод
        await Task.CompletedTask;
        return true;
    }
}