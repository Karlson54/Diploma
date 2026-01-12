using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Markets;
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
        ILogger<MarketService> logger)
        : base(repository, unitOfWork, mapper, logger)
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

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        // Можна деактивувати тільки якщо немає активних TimeEntries
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MarketId == id);

        return !hasActiveTimeEntries;
    }
}