using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.Services.Auth;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _authService = authService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _authService.RegisterAsync(
                dto.Login,
                dto.Email,
                dto.Password,
                dto.Name,
                dto.AgencyId,
                roleName: "Employee");

            var userWithDetails = await _userRepository.GetByIdWithRolesAsync(user.Id);
            if (userWithDetails == null)
                return StatusCode(500, new { Message = "Помилка при завантаженні даних користувача" });

            var roles = userWithDetails.UserRoles.Select(ur => ur.Role.Name).ToList();
            var token = await _authService.LoginAsync(dto.Login, dto.Password);

            var response = new AuthResponseDto
            {
                UserId = userWithDetails.Id,
                Login = userWithDetails.Login,
                Email = userWithDetails.Email,
                Name = userWithDetails.Name,
                AgencyId = userWithDetails.AgencyId,
                AgencyName = userWithDetails.Agency?.Name ?? string.Empty,
                Roles = roles,
                Token = token ?? string.Empty,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };

            return CreatedAtAction(
                nameof(Register), 
                new { id = user.Id }, 
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
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Внутрішня помилка сервера", Details = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var token = await _authService.LoginAsync(dto.LoginOrEmail, dto.Password);
            
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Message = "Не вдалося згенерувати токен" });

            var user = await _userRepository.GetByEmailAsync(dto.LoginOrEmail) 
                       ?? await _userRepository.GetByLoginAsync(dto.LoginOrEmail);

            if (user == null)
                return Unauthorized(new { Message = "Користувача не знайдено" });

            var userWithDetails = await _userRepository.GetByIdWithRolesAsync(user.Id);
            if (userWithDetails == null)
                return StatusCode(500, new { Message = "Помилка при завантаженні даних користувача" });

            var roles = userWithDetails.UserRoles
                .Where(ur => ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList();

            var response = new AuthResponseDto
            {
                UserId = userWithDetails.Id,
                Login = userWithDetails.Login,
                Email = userWithDetails.Email,
                Name = userWithDetails.Name,
                AgencyId = userWithDetails.AgencyId,
                AgencyName = userWithDetails.Agency?.Name ?? string.Empty,
                Roles = roles,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Внутрішня помилка сервера", Details = ex.Message });
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
}