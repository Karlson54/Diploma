using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimeTracker.Core.Common;
using TimeTracker.Core.Services.Auth;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(IAuthService authService, IOptions<JwtSettings> jwtSettings)
    {
        _authService = authService;
        _jwtSettings = jwtSettings.Value;
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
                request.AgencyId);

            return Ok(new
            {
                Success = true,
                Message = "User registered successfully",
                Data = new
                {
                    user.Id,
                    user.Login,
                    user.Email,
                    user.Name,
                    user.AgencyId,
                    HashedPassword = user.PasswordHash
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
            var token = await _authService.LoginAsync(request.LoginOrEmail, request.Password);

            return Ok(new
            {
                Success = true,
                Token = token,
                SecretKey = _jwtSettings.SecretKey // только для разработки, в продакшене так нельзя!
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

public class RegisterRequest
{
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AgencyId { get; set; }
}

public class LoginRequest
{
    public string LoginOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}