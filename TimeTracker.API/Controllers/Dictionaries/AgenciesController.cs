using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TimeTracker.API.Extensions;
using TimeTracker.Core.DTOs.Dictionaries.Agencies;
using TimeTracker.Core.Services.Dictionaries.Agencies;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class AgenciesController : BaseDictionaryController<AgencyDto, CreateAgencyDto, UpdateAgencyDto>
{
    private readonly IAgencyService _agencyService;

    public AgenciesController(
        IAgencyService service,
        ILogger<AgenciesController> logger,
        IMemoryCache cache)
        : base(service, logger, "Agency", cache, CacheKeys.AgenciesAll, CacheKeys.AgenciesActive)
    {
        _agencyService = service;
    }

    [HttpGet("{id}/users-count")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersCount(long id)
    {
        var count = await _agencyService.GetUsersCountAsync(id);
        return Ok(new { UsersCount = count });
    }

    [HttpGet("{id}/has-active-users")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HasActiveUsers(long id)
    {
        var hasActive = await _agencyService.HasActiveUsersAsync(id);
        return Ok(new { HasActiveUsers = hasActive });
    }

    [HttpGet("by-country/{country}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCountry(string country)
    {
        var agencies = await _agencyService.GetByCountryAsync(country);
        return Ok(agencies);
    }
}