// TimeTracker.API/Controllers/Auth/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.Services.Auth;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <returns>JWT токен при успешной регистрации</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { 
                    error = result.ErrorMessage,
                    success = false 
                });
            }

            // Если регистрация успешна, сразу залогиним пользователя
            var loginResult = await _authService.LoginAsync(new LoginRequest
            {
                EmailOrLogin = request.Email,
                Password = request.Password
            });

            if (!loginResult.IsSuccess)
            {
                return Ok(new { 
                    message = "User registered successfully, but login failed",
                    success = true 
                });
            }

            _logger.LogInformation("User {Email} registered and logged in successfully", request.Email);

            return Ok(new
            {
                message = "Registration successful",
                success = true,
                user = new
                {
                    id = loginResult.UserId,
                    name = loginResult.UserName,
                    email = loginResult.UserEmail
                },
                tokens = new
                {
                    accessToken = loginResult.AccessToken,
                    refreshToken = loginResult.RefreshToken,
                    expiresAt = loginResult.ExpiresAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", request.Email);
            return StatusCode(500, new { 
                error = "Internal server error",
                success = false 
            });
        }
    }

    /// <summary>
    /// Вход в систему
    /// </summary>
    /// <param name="request">Email/Login и пароль</param>
    /// <returns>JWT токен при успешном входе</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(new { 
                    error = result.ErrorMessage,
                    success = false 
                });
            }

            _logger.LogInformation("User {Email} logged in successfully", result.UserEmail);

            return Ok(new
            {
                message = "Login successful",
                success = true,
                user = new
                {
                    id = result.UserId,
                    name = result.UserName,
                    email = result.UserEmail
                },
                tokens = new
                {
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    expiresAt = result.ExpiresAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {EmailOrLogin}", request.EmailOrLogin);
            return StatusCode(500, new { 
                error = "Internal server error",
                success = false 
            });
        }
    }

    /// <summary>
    /// Тестовый endpoint для проверки JWT токена
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var userName = User.Identity?.Name;
        var email = User.FindFirst("email")?.Value;
        var roles = User.FindAll("role").Select(c => c.Value);

        return await Task.FromResult(Ok(new
        {
            message = "Token is valid!",
            user = new
            {
                id = userId,
                name = userName,
                email = email,
                roles = roles
            },
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        }));
    }
}