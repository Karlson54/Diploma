using Microsoft.AspNetCore.Identity;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<User> RegisterAsync(string login, string email, string password, string name, long agencyId)
    {
        var user = new User
        {
            Login = login,
            Email = email,
            Name = name,
            AgencyId = agencyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };


        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<string?> LoginAsync(string loginOrEmail, string password)
    {
        var user = await _userRepository.GetByEmailAsync(loginOrEmail)
                   ?? await _userRepository.GetByLoginAsync(loginOrEmail);

        if (user == null || !user.IsActive)
            return null;

        return _jwtTokenService.GenerateToken(user);
    
        // return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }
}