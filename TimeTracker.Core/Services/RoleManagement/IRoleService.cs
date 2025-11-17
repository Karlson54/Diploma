using TimeTracker.Core.DTOs.Roles;

namespace TimeTracker.Core.Services.RoleManagement;

public interface IRoleService
{
    Task<RoleDetailDto?> GetByIdAsync(long id);
    Task<RoleDetailDto?> GetByNameAsync(string name);
    Task<IEnumerable<RoleListItemDto>> GetAllAsync();
    Task<IEnumerable<RoleListItemDto>> GetActiveRolesAsync();
    
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(long id, UpdateRoleDto dto);
    Task DeleteAsync(long id);
    
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId);
    Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(long roleId);
    Task<bool> UserHasRoleAsync(long userId, string roleName);
    
    Task AssignRoleToUserAsync(long userId, long roleId);
    Task RemoveRoleFromUserAsync(long userId, long roleId);
    Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds);
    
    Task<bool> IsRoleNameExistsAsync(string name, long? excludeRoleId = null);
    Task<bool> CanDeleteRoleAsync(long roleId);
    
    Task<IEnumerable<string>> GetRolePermissionsAsync(long roleId);
    Task UpdateRolePermissionsAsync(long roleId, IEnumerable<string> permissions);
    
    Task ActivateAsync(long id);
    Task DeactivateAsync(long id);
}