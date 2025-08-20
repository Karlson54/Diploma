// TimeTracker.Data/Repositories/Users/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Users;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    #region Поиск пользователей

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());
    }

    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User?> GetByIdWithRolesAsync(long id)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    #endregion

    #region Фильтрация пользователей

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByAgencyAsync(long agencyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(u => u.AgencyId == agencyId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersWithRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return Enumerable.Empty<User>();

        return await _dbSet
            .AsNoTracking()
            .Where(u => u.UserRoles.Any(ur => 
                ur.Role.Name.ToLower() == roleName.ToLower() && 
                ur.Role.IsActive))
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    #endregion

    #region Проверки существования

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> IsLoginExistsAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        return await _dbSet
            .AnyAsync(u => u.Login.ToLower() == login.ToLower());
    }

    public async Task<bool> IsEmailExistsAsync(string email, long excludeUserId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != excludeUserId);
    }

    public async Task<bool> IsLoginExistsAsync(string login, long excludeUserId)
    {
        if (string.IsNullOrWhiteSpace(login))
            return false;

        return await _dbSet
            .AnyAsync(u => u.Login.ToLower() == login.ToLower() && u.Id != excludeUserId);
    }

    #endregion

    #region Статистика

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _dbSet.CountAsync(u => u.IsActive);
    }

    public async Task<int> GetUsersCountByAgencyAsync(long agencyId)
    {
        return await _dbSet.CountAsync(u => u.AgencyId == agencyId && u.IsActive);
    }

    #endregion

    #region Пагинация с фильтрами

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(
        int pageNumber, 
        int pageSize,
        string? searchTerm = null,
        long? agencyId = null,
        bool? isActive = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        // Применяем фильтры
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Login.ToLower().Contains(term));
        }

        if (agencyId.HasValue)
        {
            query = query.Where(u => u.AgencyId == agencyId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    #endregion
}