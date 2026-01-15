using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.Clients;
using TimeTracker.Core.Services.Dictionaries.Clients;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class ClientsController : BaseDictionaryController<ClientDto, CreateClientDto, UpdateClientDto>
{
    private readonly IClientService _clientService;

    public ClientsController(
        IClientService service,
        ILogger<ClientsController> logger)
        : base(service, logger, "Client")
    {
        _clientService = service;
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var client = await _clientService.GetByEmailAsync(email);
        if (client == null)
            return NotFound(new { Message = $"Client з email '{email}' не знайдено" });

        return Ok(client);
    }

    [HttpGet("search")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm)
    {
        var clients = await _clientService.SearchByEmailOrPhoneAsync(searchTerm);
        return Ok(clients);
    }

    [HttpGet("{id}/time-entries-count")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeEntriesCount(long id)
    {
        var count = await _clientService.GetTimeEntriesCountAsync(id);
        return Ok(new { TimeEntriesCount = count });
    }

    [HttpGet("check-email")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail(
        [FromQuery] string email,
        [FromQuery] long? excludeId = null)
    {
        var exists = await _clientService.IsEmailExistsAsync(email, excludeId);
        return Ok(new { Exists = exists });
    }
}