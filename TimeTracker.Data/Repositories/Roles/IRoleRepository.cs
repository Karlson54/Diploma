// TimeTracker.Data/Repositories/Roles/IRoleRepository.cs
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Roles;

public interface IRoleRepository : IRepository<Role>
{
    // Получение ролей
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetActiveRolesAsync();
    
    // Работа с пользовательскими ролями
    Task<IEnumerable<Role>> GetUserRolesAsync(long userId);
    Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName);
    Task<bool> UserHasRoleAsync(long userId, string roleName);
    
    // Управление ролями пользователей
    Task AssignRoleAsync(long userId, long roleId);
    Task RemoveRoleAsync(long userId, long roleId);
    Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds);
    
    // Проверки
    Task<bool> IsRoleNameExistsAsync(string name);
    Task<bool> IsRoleNameExistsAsync(string name, long excludeRoleId);
}