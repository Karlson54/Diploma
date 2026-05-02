using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.JobTypes;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.JobTypes;

public class JobTypeService : DictionaryService<JobType, JobTypeDto, CreateJobTypeDto, UpdateJobTypeDto>, IJobTypeService
{
    public JobTypeService(
        IDictionaryRepository<JobType> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<JobTypeService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.JobTypeId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.JobTypeId == id);

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
        var jobType = await _repository.GetByIdAsync(id);
        if (jobType == null)
        {
            throw new KeyNotFoundException($"JobType з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.JobTypeId == id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення JobType '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                jobType.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити JobType '{jobType.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            jobType.Id,
            jobType.Name,
            jobType.IsActive
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
        var jobType = await _repository.GetByIdAsync(id);
        if (jobType == null)
        {
            throw new KeyNotFoundException($"JobType з ID {id} не знайдено");
        }

        if (!jobType.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого JobType '{Name}' (ID: {Id})",
                jobType.Name, id);
            throw new InvalidOperationException("JobType вже деактивований");
        }

        // Перевірка активних TimeEntries
        var activeTimeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.JobTypeId == id);

        if (activeTimeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Неможливо деактивувати JobType '{Name}' (ID: {Id}) - є {Count} записів часу",
                jobType.Name, id, activeTimeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо деактивувати JobType '{jobType.Name}', " +
                $"оскільки до нього прив'язано {activeTimeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: jobType.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}