using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Media;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.Media;

public class MediaService : DictionaryService<Data.Entities.Media, MediaDto, CreateMediaDto, UpdateMediaDto>, IMediaService
{
    public MediaService(
        IDictionaryRepository<Data.Entities.Media> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MediaService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MediaId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MediaId == id);

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
        var media = await _repository.GetByIdAsync(id);
        if (media == null)
        {
            throw new KeyNotFoundException($"Media з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.MediaId == id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення Media '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                media.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити Media '{media.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            media.Id,
            media.Name,
            media.IsActive
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Media видалено. Id: {Id}, Name: {Name}",
            id, media.Name);

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
        var media = await _repository.GetByIdAsync(id);
        if (media == null)
        {
            throw new KeyNotFoundException($"Media з ID {id} не знайдено");
        }

        if (!media.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого Media '{Name}' (ID: {Id})",
                media.Name, id);
            throw new InvalidOperationException("Media вже деактивоване");
        }

        // Перевірка активних TimeEntries
        var activeTimeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.MediaId == id);

        if (activeTimeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Неможливо деактивувати Media '{Name}' (ID: {Id}) - є {Count} записів часу",
                media.Name, id, activeTimeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо деактивувати Media '{media.Name}', " +
                $"оскільки до нього прив'язано {activeTimeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Media деактивовано. Id: {Id}, Name: {Name}",
            id, media.Name);

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: media.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}