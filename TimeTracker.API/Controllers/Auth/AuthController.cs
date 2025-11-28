using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.Auth;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto);
            
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

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            
            _logger.LogInformation("Користувач {Email} успішно увійшов", response.Email);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

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

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto) // ✅ ЗМІНЕНО ТИП
    {
        try
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { Message = "Невалідний токен" });
            }

            await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
            
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
}