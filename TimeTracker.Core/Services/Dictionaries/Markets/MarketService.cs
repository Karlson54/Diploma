using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Markets;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.Markets;

public class MarketService : DictionaryService<Market, MarketDto, CreateMarketDto, UpdateMarketDto>, IMarketService
{
    public MarketService(
        IDictionaryRepository<Market> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MarketService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        // Перевіряємо чи немає TimeEntries з цим Market
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MarketId == id);

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
        var market = await _repository.GetByIdAsync(id);
        if (market == null)
        {
            throw new KeyNotFoundException($"Market з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.MarketId == id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення Market '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                market.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити Market '{market.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            market.Id,
            market.Name,
            market.IsActive
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
        var market = await _repository.GetByIdAsync(id);
        if (market == null)
        {
            throw new KeyNotFoundException($"Market з ID {id} не знайдено");
        }

        if (!market.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого Market '{Name}' (ID: {Id})",
                market.Name, id);
            throw new InvalidOperationException("Market вже деактивований");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: market.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}