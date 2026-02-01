using TimeTracker.Core.DTOs.Auth;

namespace TimeTracker.Core.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string ipAddress, string userAgent);
    Task ChangePasswordAsync(long userId, string currentPassword, string newPassword, string ipAddress, string userAgent);
    Task<bool> ValidatePasswordAsync(long userId, string password);
}