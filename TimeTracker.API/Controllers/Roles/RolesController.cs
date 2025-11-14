using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Core.Services.RoleManagement;

namespace TimeTracker.API.Controllers.Roles;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var roles = await _roleService.GetAllAsync();
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
            var roles = await _roleService.GetActiveRolesAsync();
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
            var role = await _roleService.GetByIdAsync(id);
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
            var role = await _roleService.GetByNameAsync(name);
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
            var role = await _roleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
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
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleService.UpdateAsync(id, dto);
            return Ok(role);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _roleService.DeleteAsync(id);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "CanViewUsers")]
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
            var users = await _roleService.GetUsersInRoleAsync(roleId);
            return Ok(users);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
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
            await _roleService.AssignRoleToUserAsync(dto.UserId, dto.RoleId);
            return Ok(new { Message = "Роль успішно призначено" });
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

    [HttpPost("remove")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            await _roleService.RemoveRoleFromUserAsync(dto.UserId, dto.RoleId);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("user/{userId}/replace")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> ReplaceUserRoles(long userId, [FromBody] UpdateUserRolesDto dto)
    {
        try
        {
            await _roleService.ReplaceUserRolesAsync(userId, dto.RoleIds);
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
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("check")]
    [Authorize(Policy = "CanViewUsers")]
    public async Task<IActionResult> CheckUserRole([FromQuery] long userId, [FromQuery] string roleName)
    {
        try
        {
            var hasRole = await _roleService.UserHasRoleAsync(userId, roleName);
            return Ok(new { HasRole = hasRole });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("check-name")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CheckRoleName([FromQuery] string name, [FromQuery] long? excludeRoleId = null)
    {
        var exists = await _roleService.IsRoleNameExistsAsync(name, excludeRoleId);
        return Ok(new { Exists = exists });
    }

    [HttpGet("{roleId}/can-delete")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CanDeleteRole(long roleId)
    {
        try
        {
            var canDelete = await _roleService.CanDeleteRoleAsync(roleId);
            return Ok(new { CanDelete = canDelete });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{roleId}/permissions")]
    [Authorize(Policy = "AdminOnly")]
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
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{roleId}/permissions")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateRolePermissions(long roleId, [FromBody] UpdateRolePermissionsDto dto)
    {
        try
        {
            await _roleService.UpdateRolePermissionsAsync(roleId, dto.Permissions);
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
            return BadRequest(new { Message = ex.Message });
        }
    }
}