using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.Services.Audit;
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
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    private const int MinPasswordLength = ValidationPatterns.PasswordMinLength;
    private const int MaxPasswordLength = ValidationPatterns.PasswordMaxLength;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IAuditService auditService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string userAgent)
    {
        var user = await FindUserByLoginOrEmailAsync(dto.LoginOrEmail);

        if (user == null)
        {
            _logger.LogWarning("Спроба входу з неіснуючим логіном/email: {LoginOrEmail}", dto.LoginOrEmail);

            // Логуємо невдалу спробу
            await _auditService.LogLoginAsync(
                userId: 0,
                userName: dto.LoginOrEmail,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "Невірний логін/email або пароль");

            throw new UnauthorizedAccessException("Невірний логін/email або пароль");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Спроба входу неактивного користувача: {UserId}", user.Id);

            await _auditService.LogLoginAsync(
                userId: user.Id,
                userName: user.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "Обліковий запис деактивовано");

            throw new UnauthorizedAccessException("Обліковий запис деактивовано. Зверніться до адміністратора.");
        }

        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Невірний пароль для користувача: {UserId}", user.Id);

            await _auditService.LogLoginAsync(
                userId: user.Id,
                userName: user.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "Невірний пароль");

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

            await _auditService.LogLoginAsync(
                userId: user.Id,
                userName: user.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "У вас немає активних ролей");

            throw new UnauthorizedAccessException("У вас немає активних ролей. Зверніться до адміністратора.");
        }

        var token = _jwtTokenService.GenerateToken(userWithRoles, activeRoles);

        // Логуємо успішний вхід
        await _auditService.LogLoginAsync(
            userId: user.Id,
            userName: user.Name,
            ipAddress: ipAddress,
            userAgent: userAgent,
            success: true);

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

        _logger.LogInformation(
            "User login successful. UserId: {UserId}, Email: {Email}, Agency: {AgencyId}, Roles: {Roles}",
            user.Id,
            user.Email,
            user.AgencyId,
            string.Join(", ", activeRoles));

        return response;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string ipAddress, string userAgent)
    {
        try
        {
            await ValidateRegistrationDataAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "Спроба реєстрації з існуючими даними. Email: {Email}, Login: {Login}, Error: {Error}",
                dto.Email, dto.Login, ex.Message);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }

        try
        {
            ValidatePasswordStrength(dto.Password);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                "Спроба реєстрації зі слабким паролем. Email: {Email}, Login: {Login}",
                dto.Email, dto.Login);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "Пароль не відповідає вимогам безпеки");

            throw;
        }

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
        {
            _logger.LogWarning(
                "Спроба реєстрації з неіснуючим Agency ID: {AgencyId}. Email: {Email}",
                dto.AgencyId, dto.Email);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: $"Agency з ID {dto.AgencyId} не знайдено");

            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");
        }

        if (!agency.IsActive)
        {
            _logger.LogWarning(
                "Спроба реєстрації в неактивному Agency. AgencyId: {AgencyId}, AgencyName: {AgencyName}, Email: {Email}",
                dto.AgencyId, agency.Name, dto.Email);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: $"Agency '{agency.Name}' неактивне");

            throw new InvalidOperationException("Неможливо зареєструватись в неактивному Agency");
        }

        var employeeRole = await _roleRepository.GetByNameAsync(SystemRoles.Employee);
        if (employeeRole == null)
        {
            _logger.LogError(
                "Системна роль '{RoleName}' не знайдена в базі даних при реєстрації користувача {Email}",
                SystemRoles.Employee, dto.Email);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: $"Системна помилка: роль '{SystemRoles.Employee}' не знайдена");

            throw new InvalidOperationException(
                $"Системна помилка: роль '{SystemRoles.Employee}' не знайдена. Зверніться до адміністратора.");
        }

        if (!employeeRole.IsActive)
        {
            _logger.LogError(
                "Системна роль '{RoleName}' деактивована при реєстрації користувача {Email}",
                SystemRoles.Employee, dto.Email);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: $"Системна помилка: роль '{SystemRoles.Employee}' деактивована");

            throw new InvalidOperationException(
                $"Системна помилка: роль '{SystemRoles.Employee}' деактивована. Зверніться до адміністратора.");
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

            user.UserRoles.Add(new UserRole
            {
                User = user,
                RoleId = employeeRole.Id
            });

            await _unitOfWork.SaveChangesAsync();

            var newValues = new
            {
                user.Login,
                user.Email,
                user.Name,
                AgencyId = user.AgencyId,
                AgencyName = agency.Name,
                IsActive = user.IsActive,
                AssignedRole = SystemRoles.Employee
            };

            await _auditService.LogCreateAsync(
                entityName: "User",
                entityId: user.Id,
                newValues: newValues,
                userId: user.Id,
                userName: user.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);

            await _auditService.LogRegistrationAsync(
                userName: user.Name,
                email: user.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true,
                userId: user.Id);

            _logger.LogInformation(
                "Новий користувач успішно зареєстрований. UserId: {UserId}, Email: {Email}, Login: {Login}, AgencyId: {AgencyId}, AgencyName: {AgencyName}",
                user.Id, user.Email, user.Login, user.AgencyId, agency.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Помилка при реєстрації користувача. Email: {Email}, Login: {Login}, AgencyId: {AgencyId}",
                dto.Email, dto.Login, dto.AgencyId);

            await _auditService.LogRegistrationAsync(
                userName: dto.Name,
                email: dto.Email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: "Помилка при збереженні даних користувача в БД");

            throw new InvalidOperationException(
                "Не вдалося створити обліковий запис. Спробуйте пізніше або зверніться до адміністратора.",
                ex);
        }

        var userWithRoles = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userWithRoles == null)
        {
            _logger.LogError("Не вдалося завантажити дані щойно створеного користувача {UserId}", user.Id);
            throw new InvalidOperationException("Помилка завантаження даних користувача");
        }

        var activeRoles = userWithRoles.UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToList();

        if (!activeRoles.Any())
        {
            _logger.LogError(
                "Користувач {UserId} не має активних ролей після реєстрації",
                user.Id);
            throw new InvalidOperationException("У користувача немає активних ролей. Зверніться до адміністратора.");
        }

        var token = _jwtTokenService.GenerateToken(userWithRoles, activeRoles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

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
            ExpiresAt = expiresAt
        };

        return response;
    }

    public async Task ChangePasswordAsync(long userId, string currentPassword, string newPassword, string ipAddress,
        string userAgent)
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

        // Логуємо зміну пароля
        await _auditService.LogPasswordChangeAsync(
            userId: userId,
            userName: user.Name,
            ipAddress: ipAddress,
            userAgent: userAgent);

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