// TimeTracker.Data/Repositories/Roles/RoleRepository.cs
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Roles;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    #region Получение ролей

    public async Task<Role?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    #endregion

    #region Работа с пользовательскими ролями

    public async Task<IEnumerable<Role>> GetUserRolesAsync(long userId)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return Enumerable.Empty<User>();

        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.Role.Name.ToLower() == roleName.ToLower() && ur.Role.IsActive)
            .Include(ur => ur.User)
            .Select(ur => ur.User)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<bool> UserHasRoleAsync(long userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        return await _context.UserRoles
            .AnyAsync(ur => 
                ur.UserId == userId && 
                ur.Role.Name.ToLower() == roleName.ToLower() &&
                ur.Role.IsActive);
    }

    #endregion

    #region Управление ролями пользователей

    public async Task AssignRoleAsync(long userId, long roleId)
    {
        // Проверяем, что роль еще не назначена
        var exists = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (!exists)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
        }
    }

    public async Task RemoveRoleAsync(long userId, long roleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
        }
    }

    public async Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds)
    {
        // Удаляем все текущие роли пользователя
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        _context.UserRoles.RemoveRange(existingRoles);

        // Добавляем новые роли
        var newUserRoles = roleIds.Select(roleId => new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.UserRoles.AddRangeAsync(newUserRoles);
    }

    #endregion

    #region Проверки

    public async Task<bool> IsRoleNameExistsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _dbSet.AnyAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> IsRoleNameExistsAsync(string name, long excludeRoleId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _dbSet
            .AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.Id != excludeRoleId);
    }

    #endregion
}