using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TimeTracker.API.Extensions;
using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;
using TimeTracker.Core.Services.Dictionaries.ContractingAgencies;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class ContractingAgenciesController : BaseDictionaryController<ContractingAgencyDto, CreateContractingAgencyDto,
    UpdateContractingAgencyDto>
{
    public ContractingAgenciesController(
        IContractingAgencyService service,
        ILogger<ContractingAgenciesController> logger,
        IMemoryCache cache)
        : base(service, logger, "ContractingAgency", cache, CacheKeys.ContractingAgenciesAll,
            CacheKeys.ContractingAgenciesActive)
    {
    }
}