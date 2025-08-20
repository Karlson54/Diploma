// TimeTracker.Data/Repositories/Users/IUserRepository.cs
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    // Поиск пользователей
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByLoginAsync(string login);
    Task<User?> GetByEmailWithRolesAsync(string email);
    Task<User?> GetByIdWithRolesAsync(long id);
    
    // Фильтрация пользователей
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersByAgencyAsync(long agencyId);
    Task<IEnumerable<User>> GetUsersWithRoleAsync(string roleName);
    
    // Проверки существования
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsLoginExistsAsync(string login);
    Task<bool> IsEmailExistsAsync(string email, long excludeUserId);
    Task<bool> IsLoginExistsAsync(string login, long excludeUserId);
    
    // Статистика пользователей
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetUsersCountByAgencyAsync(long agencyId);
    
    // Пагинация с фильтрами
    Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(
        int pageNumber, 
        int pageSize,
        string? searchTerm = null,
        long? agencyId = null,
        bool? isActive = null);
}