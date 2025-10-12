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

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<User> RegisterAsync(
        string login, 
        string email, 
        string password, 
        string name, 
        long agencyId,
        string? roleName = null)
    {
        // Проверка существования
        if (await _userRepository.IsEmailExistsAsync(email))
            throw new InvalidOperationException("Email вже використовується");

        if (await _userRepository.IsLoginExistsAsync(login))
            throw new InvalidOperationException("Login вже використовується");

        // Определяем роль (по умолчанию Employee)
        var targetRoleName = string.IsNullOrWhiteSpace(roleName) ? "Employee" : roleName;
        
        // Проверяем существование роли
        var role = await _roleRepository.GetByNameAsync(targetRoleName);
        if (role == null)
            throw new InvalidOperationException($"Роль '{targetRoleName}' не знайдена");

        if (!role.IsActive)
            throw new InvalidOperationException($"Роль '{targetRoleName}' неактивна");

        // Создаём пользователя
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

        // Назначаем роль
        await _roleRepository.AssignRoleAsync(user.Id, role.Id);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<string?> LoginAsync(string loginOrEmail, string password)
    {
        // Находим пользователя
        var user = await _userRepository.GetByEmailAsync(loginOrEmail)
                   ?? await _userRepository.GetByLoginAsync(loginOrEmail);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Невірний логін або пароль");

        // Проверяем пароль
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Невірний логін або пароль");

        // Получаем роли пользователя
        var roles = await _roleRepository.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();

        // Генерируем токен с ролями
        return _jwtTokenService.GenerateToken(user, roleNames);
    }
}