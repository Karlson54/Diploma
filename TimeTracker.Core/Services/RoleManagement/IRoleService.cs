using TimeTracker.Core.DTOs.Roles;

namespace TimeTracker.Core.Services.RoleManagement;

public interface IRoleService
{
    Task<RoleDetailDto?> GetByIdAsync(long id);
    Task<RoleDetailDto?> GetByNameAsync(string name);
    Task<IEnumerable<RoleListItemDto>> GetAllAsync();
    Task<IEnumerable<RoleListItemDto>> GetActiveRolesAsync();

    Task<RoleDto> CreateAsync(CreateRoleDto dto, long requestingUserId, string ipAddress, string userAgent);
    Task<RoleDto> UpdateAsync(long id, UpdateRoleDto dto, long requestingUserId, string ipAddress, string userAgent);
    Task DeleteAsync(long id, long requestingUserId, string ipAddress, string userAgent);

    Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId);
    Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(long roleId);
    Task<bool> UserHasRoleAsync(long userId, string roleName);

    // КРИТИЧНІ ОПЕРАЦІЇ - додано параметри для аудиту
    Task AssignRoleToUserAsync(long userId, long roleId, long requestingUserId, string ipAddress, string userAgent);
    Task RemoveRoleFromUserAsync(long userId, long roleId, long requestingUserId, string ipAddress, string userAgent);

    Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds, long requestingUserId, string ipAddress,
        string userAgent);

    Task<bool> IsRoleNameExistsAsync(string name, long? excludeRoleId = null);
    Task<bool> CanDeleteRoleAsync(long roleId);

    Task<IEnumerable<string>> GetRolePermissionsAsync(long roleId);

    // КРИТИЧНА ОПЕРАЦІЯ
    Task UpdateRolePermissionsAsync(long roleId, IEnumerable<string> permissions, long requestingUserId,
        string ipAddress, string userAgent);

    Task ActivateAsync(long id, long requestingUserId, string ipAddress, string userAgent);
    Task DeactivateAsync(long id, long requestingUserId, string ipAddress, string userAgent);
}