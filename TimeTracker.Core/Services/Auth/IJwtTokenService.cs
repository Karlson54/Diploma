using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public interface IJwtTokenService
{
    public string GenerateToken(User user);
}