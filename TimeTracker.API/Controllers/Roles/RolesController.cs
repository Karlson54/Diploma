using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.API.Controllers.Roles;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RolesController(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var roles = await _roleRepository.GetAllAsync();
            return Ok(roles);
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
            var roles = await _roleRepository.GetActiveRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { Message = $"Роль з ID {id} не знайдено" });

            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("by-name/{name}")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetByName(string name)
    {
        try
        {
            var role = await _roleRepository.GetByNameAsync(name);
            if (role == null)
                return NotFound(new { Message = $"Роль '{name}' не знайдено" });

            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { Message = "Назва ролі не може бути порожньою" });

            if (await _roleRepository.IsRoleNameExistsAsync(dto.Name))
                return Conflict(new { Message = "Роль з такою назвою вже існує" });

            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                Permissions = dto.Permissions,
                IsActive = true
            };

            await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { Message = $"Роль з ID {id} не знайдено" });

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { Message = "Назва ролі не може бути порожньою" });

            if (await _roleRepository.IsRoleNameExistsAsync(dto.Name, id))
                return Conflict(new { Message = "Роль з такою назвою вже існує" });

            role.Name = dto.Name;
            role.Description = dto.Description;
            role.Permissions = dto.Permissions;
            role.IsActive = dto.IsActive;

            _roleRepository.Update(role);
            await _unitOfWork.SaveChangesAsync();

            return Ok(role);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return NotFound(new { Message = $"Роль з ID {id} не знайдено" });

            var usersInRole = await _roleRepository.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
                return BadRequest(new { Message = "Неможливо видалити роль, яка призначена користувачам" });

            _roleRepository.Delete(role);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Роль успішно видалено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetUserRoles(long userId)
    {
        try
        {
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                return NotFound(new { Message = $"Користувача з ID {userId} не знайдено" });

            var roles = await _roleRepository.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{roleId}/users")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetUsersInRole(long roleId)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return NotFound(new { Message = $"Роль з ID {roleId} не знайдено" });

            var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("assign")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            var userExists = await _userRepository.ExistsAsync(dto.UserId);
            if (!userExists)
                return NotFound(new { Message = $"Користувача з ID {dto.UserId} не знайдено" });

            var roleExists = await _roleRepository.ExistsAsync(dto.RoleId);
            if (!roleExists)
                return NotFound(new { Message = $"Роль з ID {dto.RoleId} не знайдено" });

            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (await _roleRepository.UserHasRoleAsync(dto.UserId, role!.Name))
                return BadRequest(new { Message = "Роль вже призначена цьому користувачу" });

            await _roleRepository.AssignRoleAsync(dto.UserId, dto.RoleId);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Роль успішно призначено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("remove")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            var userExists = await _userRepository.ExistsAsync(dto.UserId);
            if (!userExists)
                return NotFound(new { Message = $"Користувача з ID {dto.UserId} не знайдено" });

            var roleExists = await _roleRepository.ExistsAsync(dto.RoleId);
            if (!roleExists)
                return NotFound(new { Message = $"Роль з ID {dto.RoleId} не знайдено" });

            await _roleRepository.RemoveRoleAsync(dto.UserId, dto.RoleId);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Роль успішно видалено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("user/{userId}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> UpdateUserRoles(long userId, [FromBody] UpdateUserRolesDto dto)
    {
        try
        {
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                return NotFound(new { Message = $"Користувача з ID {userId} не знайдено" });

            foreach (var roleId in dto.RoleIds)
            {
                var roleExists = await _roleRepository.ExistsAsync(roleId);
                if (!roleExists)
                    return NotFound(new { Message = $"Роль з ID {roleId} не знайдено" });
            }

            await _roleRepository.ReplaceUserRolesAsync(userId, dto.RoleIds);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Ролі користувача успішно оновлено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("check")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> CheckUserRole([FromQuery] long userId, [FromQuery] string roleName)
    {
        try
        {
            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
                return NotFound(new { Message = $"Користувача з ID {userId} не знайдено" });

            var hasRole = await _roleRepository.UserHasRoleAsync(userId, roleName);
            return Ok(new { HasRole = hasRole });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}