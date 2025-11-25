using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Auth;
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
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await FindUserByLoginOrEmailAsync(dto.LoginOrEmail);
        
        if (user == null)
        {
            _logger.LogWarning("Спроба входу з неіснуючим логіном/email: {LoginOrEmail}", dto.LoginOrEmail);
            throw new UnauthorizedAccessException("Невірний логін/email або пароль");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Спроба входу неактивного користувача: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Обліковий запис деактивовано. Зверніться до адміністратора.");
        }

        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Невірний пароль для користувача: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Невірний логін/email або пароль");
        }

        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
        if (userWithRoles == null)
        {
            throw new InvalidOperationException("Помилка завантаження даних користувача");
        }

        var activeRoles = userWithRoles.UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToList();

        if (!activeRoles.Any())
        {
            _logger.LogWarning("Користувач {UserId} не має активних ролей", user.Id);
            throw new UnauthorizedAccessException("У вас немає активних ролей. Зверніться до адміністратора.");
        }

        var token = _jwtTokenService.GenerateToken(userWithRoles, activeRoles);

        var response = new AuthResponseDto
        {
            UserId = userWithRoles.Id,
            Login = userWithRoles.Login,
            Email = userWithRoles.Email,
            Name = userWithRoles.Name,
            AgencyId = userWithRoles.AgencyId,
            AgencyName = userWithRoles.Agency?.Name ?? string.Empty,
            Roles = activeRoles,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };

        _logger.LogInformation("Успішний вхід користувача: {UserId} ({Email})", user.Id, user.Email);

        return response;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        await ValidateRegistrationDataAsync(dto);

        ValidatePasswordStrength(dto.Password);

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
        {
            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");
        }

        if (!agency.IsActive)
        {
            throw new InvalidOperationException("Неможливо зареєструватись в неактивному Agency");
        }

        var employeeRole = await _roleRepository.GetByNameAsync("Employee");
        if (employeeRole == null || !employeeRole.IsActive)
        {
            throw new InvalidOperationException("Роль Employee не знайдена або неактивна");
        }

        var user = new User
        {
            Login = dto.Login.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Name = dto.Name.Trim(),
            AgencyId = dto.AgencyId,
            PasswordHash = HashPassword(dto.Password),
            IsActive = true
        };

        try
        {
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _roleRepository.AssignRoleAsync(user.Id, employeeRole.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Новий користувач зареєстрований: {UserId} ({Email}), Agency: {AgencyId}", 
                user.Id, user.Email, user.AgencyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при реєстрації користувача {Email}", dto.Email);
            throw;
        }

        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(user.Id);
        if (userWithRoles == null)
        {
            throw new InvalidOperationException("Помилка завантаження даних користувача");
        }

        var roles = userWithRoles.UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToList();

        var token = _jwtTokenService.GenerateToken(userWithRoles, roles);

        var response = new AuthResponseDto
        {
            UserId = userWithRoles.Id,
            Login = userWithRoles.Login,
            Email = userWithRoles.Email,
            Name = userWithRoles.Name,
            AgencyId = userWithRoles.AgencyId,
            AgencyName = userWithRoles.Agency?.Name ?? string.Empty,
            Roles = roles,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };

        return response;
    }

    public async Task ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Неможливо змінити пароль неактивного користувача");
        }

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Невдала спроба зміни пароля користувачем {UserId}: невірний поточний пароль", userId);
            throw new UnauthorizedAccessException("Поточний пароль невірний");
        }

        if (VerifyPassword(newPassword, user.PasswordHash))
        {
            throw new ArgumentException("Новий пароль не може співпадати з поточним паролем");
        }

        ValidatePasswordStrength(newPassword);

        user.PasswordHash = HashPassword(newPassword);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Користувач {UserId} успішно змінив пароль", userId);
    }

    public async Task<bool> ValidatePasswordAsync(long userId, string password)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return VerifyPassword(password, user.PasswordHash);
    }

    private async Task<User?> FindUserByLoginOrEmailAsync(string loginOrEmail)
    {
        var normalized = loginOrEmail.Trim();
        
        var user = await _userRepository.GetByEmailAsync(normalized);
        
        if (user == null)
        {
            user = await _userRepository.GetByLoginAsync(normalized);
        }

        return user;
    }

    private async Task ValidateRegistrationDataAsync(RegisterDto dto)
    {
        if (await _userRepository.IsEmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Спроба реєстрації з існуючим email: {Email}", dto.Email);
            throw new InvalidOperationException($"Користувач з email '{dto.Email}' вже існує");
        }

        if (await _userRepository.IsLoginExistsAsync(dto.Login))
        {
            _logger.LogWarning("Спроба реєстрації з існуючим login: {Login}", dto.Login);
            throw new InvalidOperationException($"Користувач з login '{dto.Login}' вже існує");
        }
    }

    private void ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Пароль не може бути пустим");
        }

        if (password.Length < MinPasswordLength)
        {
            throw new ArgumentException($"Пароль має містити мінімум {MinPasswordLength} символів");
        }

        if (password.Length > MaxPasswordLength)
        {
            throw new ArgumentException($"Пароль не може перевищувати {MaxPasswordLength} символів");
        }

        if (!password.Any(char.IsUpper))
        {
            throw new ArgumentException("Пароль має містити хоча б одну велику літеру (A-Z)");
        }

        if (!password.Any(char.IsLower))
        {
            throw new ArgumentException("Пароль має містити хоча б одну малу літеру (a-z)");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new ArgumentException("Пароль має містити хоча б одну цифру (0-9)");
        }

        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?/~`";
        if (!password.Any(c => specialChars.Contains(c)))
        {
            throw new ArgumentException($"Пароль має містити хоча б один спеціальний символ ({specialChars})");
        }

        if (HasSequentialCharacters(password))
        {
            throw new ArgumentException("Пароль не повинен містити послідовні символи (наприклад, '123' або 'abc')");
        }

        if (HasRepeatingCharacters(password, 3))
        {
            throw new ArgumentException("Пароль не повинен містити більше 3 однакових символів підряд");
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при перевірці пароля");
            return false;
        }
    }

    private bool HasSequentialCharacters(string password)
    {
        const int sequenceLength = 3;
        
        for (int i = 0; i <= password.Length - sequenceLength; i++)
        {
            var isSequential = true;
            for (int j = 1; j < sequenceLength; j++)
            {
                if (password[i + j] != password[i + j - 1] + 1)
                {
                    isSequential = false;
                    break;
                }
            }
            
            if (isSequential)
            {
                return true;
            }
        }
        
        return false;
    }

    private bool HasRepeatingCharacters(string password, int maxRepeating)
    {
        for (int i = 0; i <= password.Length - maxRepeating; i++)
        {
            var allSame = true;
            for (int j = 1; j < maxRepeating; j++)
            {
                if (password[i + j] != password[i])
                {
                    allSame = false;
                    break;
                }
            }
            
            if (allSame)
            {
                return true;
            }
        }
        
        return false;
    }
}