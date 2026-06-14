using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Departments;
using TimeTracker.Core.Services.Departments;

namespace TimeTracker.API.Controllers.Departments;

/// <summary>
/// Управління відділами агенцій
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IDepartmentService service,
        ILogger<DepartmentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Отримати відділ за ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var department = await _service.GetByIdAsync(id);
        if (department == null)
            return NotFound(new { Message = $"Відділ з ID {id} не знайдено" });

        return Ok(department);
    }

    /// <summary>
    /// Отримати всі відділи агенції
    /// </summary>
    [HttpGet("by-agency/{agencyId}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAgency(long agencyId)
    {
        var departments = await _service.GetByAgencyAsync(agencyId);
        return Ok(departments);
    }

    /// <summary>
    /// Отримати тільки активні відділи агенції
    /// </summary>
    [HttpGet("by-agency/{agencyId}/active")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveByAgency(long agencyId)
    {
        var departments = await _service.GetActiveByAgencyAsync(agencyId);
        return Ok(departments);
    }

    /// <summary>
    /// Створити новий відділ
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "CanManageDictionaries")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var department = await _service.CreateAsync(
                dto,
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetIpAddress(),
                GetUserAgent());

            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні відділу");
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Оновити відділ
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageDictionaries")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateDepartmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var department = await _service.UpdateAsync(
                id, dto,
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetIpAddress(),
                GetUserAgent());

            return Ok(department);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні відділу {Id}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Видалити відділ (тільки якщо немає прив'язаних юзерів)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(
                id,
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetIpAddress(),
                GetUserAgent());

            return Ok(new { Message = "Відділ успішно видалено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні відділу {Id}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Активувати відділ
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "CanManageDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            await _service.ActivateAsync(
                id,
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetIpAddress(),
                GetUserAgent());

            return Ok(new { Message = "Відділ успішно активовано" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Деактивувати відділ
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "CanManageDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            await _service.DeactivateAsync(
                id,
                GetCurrentUserId(),
                GetCurrentUserName(),
                GetIpAddress(),
                GetUserAgent());

            return Ok(new { Message = "Відділ успішно деактивовано" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(claim) || !long.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Невалідний токен");
        return id;
    }

    private string GetCurrentUserName() =>
        User.FindFirst("userName")?.Value ?? "Unknown";

    private string GetIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var ua = HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        return ua.Length > 500 ? ua[..500] : ua;
    }
}