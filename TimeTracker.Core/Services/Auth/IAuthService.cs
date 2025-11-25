using TimeTracker.Core.DTOs.Auth;

namespace TimeTracker.Core.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    
    Task ChangePasswordAsync(long userId, string currentPassword, string newPassword);
    
    Task<bool> ValidatePasswordAsync(long userId, string password);
}