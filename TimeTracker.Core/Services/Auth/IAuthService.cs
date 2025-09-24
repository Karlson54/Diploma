using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public interface IAuthService
{
    Task<User> RegisterAsync(string login, string email, string password, string name, long agencyId);
    Task<string?> LoginAsync(string loginOrEmail, string password);
}