using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.AdminPermissions;
using TimeTracker.Core.Services.AdminPermissions;

namespace TimeTracker.API.Controllers.AdminPermissions;

/// <summary>
/// Управління дозволами адміністраторів (тільки SuperAdmin)
/// </summary>
[ApiController]
[Route("api/admin-permissions")]
[Authorize(Policy = "CanManageAdminPermissions")]
[Produces("application/json")]
[Tags("AdminPermissions")]
public class AdminPermissionsController : ControllerBase
{
    private readonly IAdminPermissionService _service;
    private readonly ILogger<AdminPermissionsController> _logger;

    public AdminPermissionsController(
        IAdminPermissionService service,
        ILogger<AdminPermissionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Отримати дозволи конкретного Admin-а</summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(AdminPermissionsForUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByUser(long userId)
    {
        try
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>Встановити дозволи Admin-у (повна перезапис)</summary>
    [HttpPut("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPermissions(long userId, [FromBody] SetAdminPermissionsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var requestingUserId = GetCurrentUserId();
            await _service.SetPermissionsAsync(userId, dto, requestingUserId);
            return Ok(new { Message = "Дозволи успішно оновлено" });
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

    /// <summary>Очистити всі дозволи Admin-а</summary>
    [HttpDelete("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearPermissions(long userId)
    {
        try
        {
            var requestingUserId = GetCurrentUserId();
            await _service.ClearPermissionsAsync(userId, requestingUserId);
            return Ok(new { Message = "Дозволи очищено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>Отримати власні дозволи (для поточного Admin-а)</summary>
    [HttpGet("my")]
    [Authorize(Policy = "AdminOrSuperAdmin")]
    [ProducesResponseType(typeof(AdminPermissionsForUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.GetByUserIdAsync(currentUserId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(claim) || !long.TryParse(claim, out var id))
            throw new UnauthorizedAccessException("Невалідний токен");
        return id;
    }
}