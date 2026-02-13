using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.ProjectBrands;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.ProjectBrands;

public class ProjectBrandService : DictionaryService<ProjectBrand, ProjectBrandDto, CreateProjectBrandDto, UpdateProjectBrandDto>, IProjectBrandService
{
    public ProjectBrandService(
        IDictionaryRepository<ProjectBrand> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProjectBrandService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ProjectBrandId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ProjectBrandId == id);

        return !hasActiveTimeEntries;
    }

    //DeleteAsync для детальних помилок
    public override async Task DeleteAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var projectBrand = await _repository.GetByIdAsync(id);
        if (projectBrand == null)
        {
            throw new KeyNotFoundException($"ProjectBrand з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.ProjectBrandId == id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення ProjectBrand '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                projectBrand.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити ProjectBrand '{projectBrand.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            projectBrand.Id,
            projectBrand.Name,
            projectBrand.IsActive
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "ProjectBrand видалено. Id: {Id}, Name: {Name}",
            id, projectBrand.Name);

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
        var projectBrand = await _repository.GetByIdAsync(id);
        if (projectBrand == null)
        {
            throw new KeyNotFoundException($"ProjectBrand з ID {id} не знайдено");
        }

        if (!projectBrand.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого ProjectBrand '{Name}' (ID: {Id})",
                projectBrand.Name, id);
            throw new InvalidOperationException("ProjectBrand вже деактивований");
        }

        // Перевірка активних TimeEntries
        var activeTimeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.ProjectBrandId == id);

        if (activeTimeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Неможливо деактивувати ProjectBrand '{Name}' (ID: {Id}) - є {Count} записів часу",
                projectBrand.Name, id, activeTimeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо деактивувати ProjectBrand '{projectBrand.Name}', " +
                $"оскільки до нього прив'язано {activeTimeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "ProjectBrand деактивовано. Id: {Id}, Name: {Name}",
            id, projectBrand.Name);

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: projectBrand.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}