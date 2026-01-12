using TimeTracker.Core.DTOs.Dictionaries.Markets;

namespace TimeTracker.Core.Services.Dictionaries.Markets;

public interface IMarketService : IDictionaryService<MarketDto, CreateMarketDto, UpdateMarketDto>
{
}