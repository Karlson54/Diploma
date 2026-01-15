using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.Markets;
using TimeTracker.Core.Services.Dictionaries.Markets;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class MarketsController : BaseDictionaryController<MarketDto, CreateMarketDto, UpdateMarketDto>
{
    public MarketsController(
        IMarketService service,
        ILogger<MarketsController> logger)
        : base(service, logger, "Market")
    {
    }
}