using Microsoft.Extensions.Logging;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    private const string DummyHash = "$2a$11$dummyhashtopreventtimingattacks123456789012";

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(
        string login, 
        string email, 
        string password, 
        string name, 
        long agencyId,
        string? roleName = null)
    {
        email = email.Trim().ToLowerInvariant();
        login = login.Trim().ToLowerInvariant();
        name = name.Trim();

        ValidatePasswordStrength(password);

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            throw new InvalidOperationException("Email вже використовується");

        existingUser = await _userRepository.GetByLoginAsync(login);
        if (existingUser != null)
            throw new InvalidOperationException("Login вже використовується");

        var agency = await _unitOfWork.Agencies.GetByIdAsync(agencyId);
        if (agency == null)
            throw new KeyNotFoundException($"Agency з ID {agencyId} не знайдено");

        if (!agency.IsActive)
            throw new InvalidOperationException("Неможливо створити користувача для неактивного Agency");

        var targetRoleName = string.IsNullOrWhiteSpace(roleName) ? "Employee" : roleName;
        var role = await _roleRepository.GetByNameAsync(targetRoleName);
        
        if (role == null)
            throw new InvalidOperationException($"Роль '{targetRoleName}' не знайдена");

        if (!role.IsActive)
            throw new InvalidOperationException($"Роль '{targetRoleName}' неактивна");

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
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

            await _roleRepository.AssignRoleAsync(user.Id, role.Id);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "User registered successfully: {UserId}, Email: {Email}, Role: {Role}", 
                user.Id, email, targetRoleName);

            return user;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError("Failed to register user: {Email}", email);
            throw;
        }
    }

    public async Task<string?> LoginAsync(string loginOrEmail, string password)
    {
        loginOrEmail = loginOrEmail.Trim().ToLowerInvariant();

        _logger.LogInformation("Login attempt for: {LoginOrEmail}", loginOrEmail);

        var user = await _userRepository.GetByEmailAsync(loginOrEmail)
                   ?? await _userRepository.GetByLoginAsync(loginOrEmail);

        if (user == null)
        {
            BCrypt.Net.BCrypt.Verify(password, DummyHash);
            _logger.LogWarning("Failed login - user not found: {LoginOrEmail}", loginOrEmail);
            throw new UnauthorizedAccessException("Невірні облікові дані");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Failed login - inactive user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Обліковий запис деактивовано. Зверніться до адміністратора");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login - wrong password for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Невірні облікові дані");
        }

        var roles = await _roleRepository.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();

        var token = _jwtTokenService.GenerateToken(user, roleNames);

        _logger.LogInformation(
            "Successful login for user: {UserId}, Roles: {Roles}", 
            user.Id, string.Join(", ", roleNames));

        return token;
    }

    private void ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не може бути порожнім");

        if (password.Length < 8)
            throw new ArgumentException("Пароль має містити мінімум 8 символів");

        if (password.Length > 100)
            throw new ArgumentException("Пароль не може перевищувати 100 символів");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Пароль має містити хоча б одну велику літеру (A-Z)");

        if (!password.Any(char.IsLower))
            throw new ArgumentException("Пароль має містити хоча б одну малу літеру (a-z)");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Пароль має містити хоча б одну цифру (0-9)");

        if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?/~`".Contains(ch)))
            throw new ArgumentException("Пароль має містити хоча б один спеціальний символ (!@#$%^&* тощо)");

        var commonPasswords = new[] { "password", "12345678", "qwerty123", "admin123" };
        if (commonPasswords.Any(p => password.ToLowerInvariant().Contains(p)))
            throw new ArgumentException("Пароль занадто простий. Використовуйте більш складну комбінацію");
    }
}