using TimeTracker.Core.DTOs.Users;

namespace TimeTracker.Core.Services.UserManagement;

public interface IUserService
{
    Task<UserDetailDto?> GetByIdAsync(long id);
    Task<IEnumerable<UserListItemDto>> GetAllAsync();
    Task<IEnumerable<UserListItemDto>> GetActiveUsersAsync();
    Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        long? agencyId = null,
        bool? isActive = null);
    
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(long id, UpdateUserDto dto);
    
    Task UpdateUserRolesAsync(long userId, List<long> roleIds);
    
    Task ActivateAsync(long id);
    Task DeactivateAsync(long id);
    
    Task ChangePasswordAsync(long userId, ChangePasswordDto dto);
    
    Task<bool> IsEmailExistsAsync(string email, long? excludeUserId = null);
    Task<bool> IsLoginExistsAsync(string login, long? excludeUserId = null);
}