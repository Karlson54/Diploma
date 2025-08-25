using TimeTracker.Core.DTOs.Auth;

namespace TimeTracker.Core.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
}