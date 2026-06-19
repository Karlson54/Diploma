using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.UserManagement;

namespace TimeTracker.API.Controllers.Users;

/// <summary>
/// Управління користувачами системи
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Отримати користувача за ID
    /// </summary>
    /// <param name="id">Ідентифікатор користувача</param>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = $"Користувача з ID {id} не знайдено" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні користувача {UserId}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати всіх користувачів
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні списку користувачів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати тільки активних користувачів
    /// </summary>
    [HttpGet("active")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні активних користувачів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати користувачів з пагінацією та фільтрацією
    /// </summary>
    /// <param name="pageNumber">Номер сторінки (починаючи з 1)</param>
    /// <param name="pageSize">Кількість записів на сторінці</param>
    /// <param name="searchTerm">Пошук за іменем, email або логіном</param>
    /// <param name="agencyId">Фільтр за агентством</param>
    /// <param name="isActive">Фільтр за статусом активності</param>
    [HttpGet("paged")]
    [Authorize(Policy = "CanViewUsers")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] long? agencyId = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var (users, totalCount) = await _userService.GetPagedAsync(
                pageNumber, pageSize, searchTerm, agencyId, isActive);

            return Ok(new
            {
                Data = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні сторінки користувачів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Створити нового користувача
    /// </summary>
    /// <param name="dto">Дані нового користувача</param>
    [HttpPost]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Отримуємо контекст запиту для аудиту
            var requestingUserId = GetCurrentUserId();
            var requestingUserName = GetCurrentUserName();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Передаємо параметри аудиту
            var user = await _userService.CreateAsync(
                dto,
                requestingUserId,
                requestingUserName,
                ipAddress,
                userAgent);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні користувача");
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Оновити дані користувача
    /// </summary>
    /// <param name="id">Ідентифікатор користувача</param>
    /// <param name="dto">Нові дані користувача</param>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Отримуємо контекст запиту для аудиту
            var requestingUserId = GetCurrentUserId();
            var requestingUserName = GetCurrentUserName();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Передаємо параметри аудиту
            var user = await _userService.UpdateAsync(
                id,
                dto,
                requestingUserId,
                requestingUserName,
                ipAddress,
                userAgent);

            return Ok(user);
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
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні користувача {UserId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Активувати деактивованого користувача
    /// </summary>
    /// <param name="id">Ідентифікатор користувача</param>
    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            // Отримуємо контекст запиту для аудиту
            var requestingUserId = GetCurrentUserId();
            var requestingUserName = GetCurrentUserName();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Передаємо параметри аудиту
            await _userService.ActivateAsync(
                id,
                requestingUserId,
                requestingUserName,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Користувача активовано" });
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
            _logger.LogError(ex, "Помилка при активації користувача {UserId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Деактивувати користувача (soft delete)
    /// </summary>
    /// <param name="id">Ідентифікатор користувача</param>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "CanManageUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            // Отримуємо контекст запиту для аудиту
            var requestingUserId = GetCurrentUserId();
            var requestingUserName = GetCurrentUserName();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Передаємо параметри аудиту
            await _userService.DeactivateAsync(
                id,
                requestingUserId,
                requestingUserName,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Користувача деактивовано" });
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
            _logger.LogError(ex, "Помилка при деактивації користувача {UserId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Змінити пароль користувача. Доступно самому користувачу або Admin
    /// </summary>
    /// <param name="id">Ідентифікатор користувача</param>
    /// <param name="dto">Поточний та новий пароль</param>
    [HttpPost("{id}/change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            // Перевірка прав: тільки сам користувач або Admin
            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning(
                    "Користувач {RequestingUserId} намагається змінити пароль користувача {TargetUserId}",
                    currentUserId, id);
                return Forbid();
            }

            // Отримуємо контекст запиту для аудиту
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Передаємо параметри аудиту
            await _userService.ChangePasswordAsync(
                id,
                dto,
                currentUserId,
                currentUserName,
                ipAddress,
                userAgent);

            return Ok(new { Message = "Пароль успішно змінено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при зміні пароля користувача {UserId}", id);
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    /// <summary>
    /// Перевірити чи існує email (для валідації форм)
    /// </summary>
    /// <param name="email">Email для перевірки</param>
    /// <param name="excludeUserId">ID користувача якого виключити з перевірки (для редагування)</param>
    [HttpGet("check-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] long? excludeUserId = null)
    {
        var exists = await _userService.IsEmailExistsAsync(email, excludeUserId);
        return Ok(new { Exists = exists });
    }

    /// <summary>
    /// Перевірити чи існує логін (для валідації форм)
    /// </summary>
    /// <param name="login">Логін для перевірки</param>
    /// <param name="excludeUserId">ID користувача якого виключити з перевірки (для редагування)</param>
    [HttpGet("check-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckLogin([FromQuery] string login, [FromQuery] long? excludeUserId = null)
    {
        var exists = await _userService.IsLoginExistsAsync(login, excludeUserId);
        return Ok(new { Exists = exists });
    }

    /// <summary>
    /// Отримати профіль поточного користувача
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(currentUserId);
            if (user == null)
                return NotFound(new { Message = "Користувача не знайдено" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні профілю");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Оновити профіль поточного користувача (ім'я, email, логін)
    /// </summary>
    [HttpPut("me/profile")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Проверяем роль из JWT клейма
            var isAdmin = User.IsInRole("Admin");

            var user = await _userService.UpdateProfileAsync(
                currentUserId,
                dto,
                ipAddress,
                userAgent,
                isAdmin);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні профілю");
            return StatusCode(500, new { Message = "Внутрішня помилка сервера" });
        }
    }

    // ==================== HELPER METHODS ====================

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

    private string GetCurrentUserName()
    {
        var userName = User.FindFirst("userName")?.Value;

        if (string.IsNullOrEmpty(userName))
        {
            _logger.LogWarning("Невалідний токен: не вдалося отримати userName");
            return "Unknown";
        }

        return userName;
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