using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TimeTracker.API.Extensions;
using TimeTracker.Core.DTOs.Dictionaries.Markets;
using TimeTracker.Core.Services.Dictionaries.Markets;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class MarketsController : BaseDictionaryController<MarketDto, CreateMarketDto, UpdateMarketDto>
{
    public MarketsController(
        IMarketService service,
        ILogger<MarketsController> logger,
        IMemoryCache cache)
        : base(service, logger, "Market", cache, CacheKeys.MarketsAll, CacheKeys.MarketsActive)
    {
    }
}