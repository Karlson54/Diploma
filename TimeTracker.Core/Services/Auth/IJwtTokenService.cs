using System.Security.Claims;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public interface IJwtTokenService
{
    Task<string> GenerateAccessTokenAsync(User user, IEnumerable<Role> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true);
    Task<bool> ValidateTokenAsync(string token);
    Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token);
    Task<long> GetUserIdFromTokenAsync(string token);
    Task<bool> IsTokenExpiredAsync(string token);
}