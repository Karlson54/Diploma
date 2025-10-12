using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Services.Auth;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.RegisterAsync(
                request.Login,
                request.Email,
                request.Password,
                request.Name,
                request.AgencyId,
                request.RoleName); // ← Передаём роль

            return Ok(new
            {
                Success = true,
                Message = "Користувач успішно зареєстрований",
                Data = new
                {
                    user.Id,
                    user.Login,
                    user.Email,
                    user.Name,
                    user.AgencyId,
                    Role = request.RoleName
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(
                request.LoginOrEmail, 
                request.Password);

            return Ok(new
            {
                Success = true,
                Token = token
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}

// DTO для регистрации
public class RegisterRequest
{
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AgencyId { get; set; }
    public string? RoleName { get; set; } // ← НОВОЕ: Опциональная роль (если null - Employee по умолчанию)
}

// DTO для логина
public class LoginRequest
{
    public string LoginOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}