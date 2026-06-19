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

    Task<UserDto> CreateAsync(
        CreateUserDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task<UserDto> UpdateAsync(
        long id,
        UpdateUserDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task ActivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task DeactivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task ChangePasswordAsync(
        long userId,
        ChangePasswordDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task<UserDto> UpdateProfileAsync(
        long userId,
        UpdateProfileDto dto,
        string ipAddress,
        string userAgent,
        bool isAdmin = false);

    Task<bool> IsEmailExistsAsync(string email, long? excludeUserId = null);
    Task<bool> IsLoginExistsAsync(string login, long? excludeUserId = null);
}