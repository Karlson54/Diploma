using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        if (request == null)
            return AuthResult.Failure("Invalid request");

        // Найти пользователя по email или login
        var user = await _userRepository.GetByEmailAsync(request.EmailOrLogin) ??
                   await _userRepository.GetByLoginAsync(request.EmailOrLogin);

        if (user == null || !user.IsActive)
            return AuthResult.Failure("Invalid credentials");

        // Проверить пароль
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return AuthResult.Failure("Invalid credentials");

        // Получить роли пользователя
        var userRoles = await _roleRepository.GetUserRolesAsync(user.Id);

        // Генерировать токены
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, userRoles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Сохранить refresh token (в реальном проекте - в отдельной таблице)
        // Здесь упрощенный пример
        
        return AuthResult.Success(accessToken, refreshToken, user.Id, user.Name, user.Email);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthResult.Failure("Invalid refresh token");

        // В реальном проекте нужно проверить refresh token в БД
        // Здесь упрощенная логика
        
        return AuthResult.Failure("Refresh token logic not implemented");
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        // Удалить refresh token из БД
        // Здесь упрощенная логика
        
        return await Task.FromResult(true);
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
            return AuthResult.Failure("Invalid request");

        // Проверить, что email уникальный
        if (await _userRepository.IsEmailExistsAsync(request.Email))
            return AuthResult.Failure("Email already exists");

        // Проверить, что login уникальный
        if (await _userRepository.IsLoginExistsAsync(request.Login))
            return AuthResult.Failure("Login already exists");

        // Создать пользователя
        var user = new TimeTracker.Data.Entities.User
        {
            Name = request.Name,
            Email = request.Email,
            Login = request.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AgencyId = request.AgencyId,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Назначить базовую роль (например, Employee)
        var employeeRole = await _roleRepository.GetByNameAsync("Employee");
        if (employeeRole != null)
        {
            await _roleRepository.AssignRoleAsync(user.Id, employeeRole.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        return AuthResult.Success("User registered successfully");
    }
}