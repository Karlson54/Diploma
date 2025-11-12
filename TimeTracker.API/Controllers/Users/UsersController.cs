using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.UserManagement;

namespace TimeTracker.API.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewUsers")]
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("active")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetActive()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("paged")]
    [Authorize(Policy = "CanViewUsers")]
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateAsync(dto);
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
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateAsync(id, dto);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            await _userService.ActivateAsync(id);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            await _userService.DeactivateAsync(id);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            var currentUserId = long.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (currentUserId != id && !User.IsInRole("Admin"))
                return Forbid();

            await _userService.ChangePasswordAsync(id, dto);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] long? excludeUserId = null)
    {
        var exists = await _userService.IsEmailExistsAsync(email, excludeUserId);
        return Ok(new { Exists = exists });
    }

    [HttpGet("check-login")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckLogin([FromQuery] string login, [FromQuery] long? excludeUserId = null)
    {
        var exists = await _userService.IsLoginExistsAsync(login, excludeUserId);
        return Ok(new { Exists = exists });
    }
}