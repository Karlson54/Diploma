using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;
using TimeTracker.Core.Services.Dictionaries.ContractingAgencies;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class ContractingAgenciesController : BaseDictionaryController<ContractingAgencyDto, CreateContractingAgencyDto, UpdateContractingAgencyDto>
{
    public ContractingAgenciesController(
        IContractingAgencyService service,
        ILogger<ContractingAgenciesController> logger)
        : base(service, logger, "ContractingAgency")
    {
    }
}