using TimeTracker.Core.DTOs.Roles;

namespace TimeTracker.Core.Services.RoleManagement;

public interface IRoleService
{
    // Получение ролей
    Task<RoleDetailDto?> GetByIdAsync(long id);
    Task<RoleDetailDto?> GetByNameAsync(string name);
    Task<IEnumerable<RoleListItemDto>> GetAllAsync();
    Task<IEnumerable<RoleListItemDto>> GetActiveRolesAsync();
    
    // CRUD операции
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(long id, UpdateRoleDto dto);
    Task DeleteAsync(long id);
    
    // Управление ролями пользователей
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId);
    Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(long roleId);
    Task<bool> UserHasRoleAsync(long userId, string roleName);
    
    // Назначение/отзыв ролей
    Task AssignRoleToUserAsync(long userId, long roleId);
    Task RemoveRoleFromUserAsync(long userId, long roleId);
    Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds);
    
    // Валидация
    Task<bool> IsRoleNameExistsAsync(string name, long? excludeRoleId = null);
    Task<bool> CanDeleteRoleAsync(long roleId);
    
    // Работа с permissions
    Task<IEnumerable<string>> GetRolePermissionsAsync(long roleId);
    Task UpdateRolePermissionsAsync(long roleId, IEnumerable<string> permissions);
}