using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.Auth;
using Microsoft.AspNetCore.RateLimiting;
using TimeTracker.API.Extensions;

namespace TimeTracker.API.Controllers.Auth;

/// <summary>
/// Аутентифікація та управління сесіями користувачів
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Реєстрація нового користувача
    /// </summary>
    /// <param name="dto">Дані для реєстрації</param>
    /// <returns>JWT токен та інформація про користувача</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var response = await _authService.RegisterAsync(dto, ipAddress, userAgent);

            _logger.LogInformation("Користувач {Email} успішно зареєстрований", dto.Email);

            return CreatedAtAction(
                nameof(ValidateToken),
                new { userId = response.UserId },
                response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Вхід до системи
    /// </summary>
    /// <param name="dto">Логін та пароль</param>
    /// <returns>JWT токен та інформація про користувача</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var response = await _authService.LoginAsync(dto, ipAddress, userAgent);

            _logger.LogInformation("Користувач {Email} успішно увійшов", response.Email);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Перевірка валідності JWT токену
    /// </summary>
    /// <returns>Інформація про поточного користувача з токену</returns>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ValidateToken()
    {
        var userId = User.FindFirst("userId")?.Value;
        var userName = User.FindFirst("userName")?.Value;
        var roles = User.FindAll("role").Select(c => c.Value).ToList();

        return Ok(new
        {
            Valid = true,
            UserId = userId,
            UserName = userName,
            Roles = roles,
            Message = "Токен валідний"
        });
    }

    /// <summary>
    /// Зміна пароля поточного користувача
    /// </summary>
    /// <param name="dto">Поточний та новий пароль</param>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Невалідний токен" });
            }

            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword, ipAddress, userAgent);

            _logger.LogInformation("Користувач {UserId} успішно змінив пароль", userId);

            return Ok(new { Message = "Пароль успішно змінено" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Невдала спроба зміни пароля: {Message}", ex.Message);
            return Unauthorized(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    private string GetIpAddress()
    {
        // Спочатку перевіряємо заголовки проксі/load balancer
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For може містити список IP адрес, беремо першу
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Перевіряємо інші стандартні заголовки
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Якщо заголовків немає, беремо IP з Connection
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Обмежуємо довжину для збереження в БД (max 500 символів згідно конфігурації)
        return userAgent.Length > 500
            ? userAgent.Substring(0, 500)
            : userAgent;
    }
}