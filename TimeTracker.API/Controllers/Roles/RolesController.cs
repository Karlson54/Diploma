using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Core.Services.RoleManagement;

namespace TimeTracker.API.Controllers.Roles;

/// <summary>
/// Управління ролями та правами доступу
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        IRoleService roleService,
        ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Отримати всі ролі
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<RoleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Отримати тільки активні ролі
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<RoleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActive()
    {
        var roles = await _roleService.GetActiveRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Отримати роль за ID
    /// </summary>
    /// <param name="id">Ідентифікатор ролі</param>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(long id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null)
            return NotFound(new { Message = $"Роль з ID {id} не знайдено" });

        return Ok(role);
    }

    [HttpGet("by-name/{name}")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(RoleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByName(string name)
    {
        var role = await _roleService.GetByNameAsync(name);
        if (role == null)
            return NotFound(new { Message = $"Роль '{name}' не знайдено" });

        return Ok(role);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserRoles(long userId)
    {
        try
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати список користувачів з певною роллю
    /// </summary>
    /// <param name="roleId">Ідентифікатор ролі</param>
    [HttpGet("{roleId}/users")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<UserInRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsersInRole(long roleId)
    {
        try
        {
            var users = await _roleService.GetUsersInRoleAsync(roleId);
            return Ok(users);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати permissions ролі у вигляді списку рядків
    /// </summary>
    /// <param name="roleId">Ідентифікатор ролі</param>
    [HttpGet("{roleId}/permissions")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRolePermissions(long roleId)
    {
        try
        {
            var permissions = await _roleService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Перевірити чи має користувач певну роль
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <param name="roleName">Назва ролі</param>
    [HttpGet("check")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckUserRole(
        [FromQuery] long userId,
        [FromQuery] string roleName)
    {
        var hasRole = await _roleService.UserHasRoleAsync(userId, roleName);
        return Ok(new { HasRole = hasRole });
    }

    /// <summary>
    /// Перевірити чи існує назва ролі (для валідації форм)
    /// </summary>
    /// <param name="name">Назва для перевірки</param>
    /// <param name="excludeRoleId">ID ролі якого виключити з перевірки</param>
    [HttpGet("check-name")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckRoleName(
        [FromQuery] string name,
        [FromQuery] long? excludeRoleId = null)
    {
        var exists = await _roleService.IsRoleNameExistsAsync(name, excludeRoleId);
        return Ok(new { Exists = exists });
    }

    [HttpGet("{roleId}/can-delete")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CanDeleteRole(long roleId)
    {
        var canDelete = await _roleService.CanDeleteRoleAsync(roleId);
        return Ok(new { CanDelete = canDelete });
    }

    /// <summary>
    /// Створити нову роль
    /// </summary>
    /// <param name="dto">Дані нової ролі</param>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            var role = await _roleService.CreateAsync(dto, requestingUserId, ipAddress, userAgent);

            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні ролі");
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Оновити роль
    /// </summary>
    /// <param name="id">Ідентифікатор ролі</param>
    /// <param name="dto">Нові дані ролі</param>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            var role = await _roleService.UpdateAsync(id, dto, requestingUserId, ipAddress, userAgent);

            return Ok(role);
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
            _logger.LogError(ex, "Помилка при оновленні ролі {RoleId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Видалити роль (тільки якщо не призначена жодному користувачу)
    /// </summary>
    /// <param name="id">Ідентифікатор ролі</param>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.DeleteAsync(id, requestingUserId, ipAddress, userAgent);

            return Ok(new { Message = "Роль успішно видалено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Неможливо видалити роль {RoleId}", id);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні ролі {RoleId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Активувати роль
    /// </summary>
    /// <param name="id">Ідентифікатор ролі</param>
    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.ActivateAsync(id, requestingUserId, ipAddress, userAgent);

            return Ok(new { Message = "Роль успішно активовано" });
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
            _logger.LogError(ex, "Помилка при активації ролі {RoleId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Деактивувати роль
    /// </summary>
    /// <param name="id">Ідентифікатор ролі</param>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.DeactivateAsync(id, requestingUserId, ipAddress, userAgent);

            return Ok(new { Message = "Роль успішно деактивовано" });
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
            _logger.LogError(ex, "Помилка при деактивації ролі {RoleId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Призначити роль користувачу
    /// </summary>
    /// <param name="dto">ID користувача та ID ролі</param>
    [HttpPost("assign")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.AssignRoleToUserAsync(
                dto.UserId,
                dto.RoleId,
                requestingUserId,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Роль успішно призначено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Конфлікт при призначенні ролі {RoleId} користувачу {UserId}", dto.RoleId,
                dto.UserId);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при призначенні ролі {RoleId} користувачу {UserId}", dto.RoleId, dto.UserId);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    [HttpPost("remove")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.RemoveRoleFromUserAsync(
                dto.UserId,
                dto.RoleId,
                requestingUserId,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Роль успішно видалено" });
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
            _logger.LogError(ex, "Помилка при видаленні ролі {RoleId} у користувача {UserId}", dto.RoleId, dto.UserId);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Замінити всі ролі користувача
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <param name="dto">Новий список ID ролей</param>
    [HttpPut("user/{userId}/replace")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReplaceUserRoles(long userId, [FromBody] UpdateUserRolesDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.ReplaceUserRolesAsync(
                userId,
                dto.RoleIds,
                requestingUserId,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Ролі користувача успішно оновлено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при заміні ролей користувача {UserId}", userId);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Оновити permissions ролі
    /// </summary>
    /// <param name="roleId">Ідентифікатор ролі</param>
    /// <param name="dto">Новий набір permissions</param>
    [HttpPut("{roleId}/permissions")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRolePermissions(long roleId, [FromBody] UpdateRolePermissionsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            //Получаем контекст запроса для аудита
            var requestingUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            //Передаем параметры аудита
            await _roleService.UpdateRolePermissionsAsync(
                roleId,
                dto.Permissions,
                requestingUserId,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Permissions успішно оновлено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні permissions ролі {RoleId}", roleId);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Невалідний токен: не вдалося отримати userId");
            throw new UnauthorizedAccessException("Невалідний токен");
        }

        return userId;
    }

    private string GetIpAddress()
    {
        // Перевіряємо X-Forwarded-For (якщо за proxy/load balancer)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Перевіряємо X-Real-IP
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Використовуємо RemoteIpAddress
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Обмежуємо розмір (БД constraint 500 символів)
        return userAgent.Length > 500
            ? userAgent.Substring(0, 500)
            : userAgent;
    }
}