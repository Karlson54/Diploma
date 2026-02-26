using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.UserManagement;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditService auditService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<UserDetailDto?> GetByIdAsync(long id)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(id);
        if (user == null)
            return null;

        return _mapper.Map<UserDetailDto>(user);
    }

    public async Task<IEnumerable<UserListItemDto>> GetAllAsync()
    {
        var users = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserListItemDto>>(users);
    }

    public async Task<IEnumerable<UserListItemDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserListItemDto>>(users);
    }

    public async Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        long? agencyId = null,
        bool? isActive = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Login.ToLower().Contains(term));
        }

        if (agencyId.HasValue)
        {
            query = query.Where(u => u.AgencyId == agencyId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = _mapper.Map<IEnumerable<UserListItemDto>>(users);

        return (userDtos, totalCount);
    }

    public async Task<UserDto> CreateAsync(
        CreateUserDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        // Валідація - логуємо в консоль
        if (await _userRepository.IsEmailExistsAsync(dto.Email))
        {
            _logger.LogWarning(
                "Спроба створення користувача з існуючим email: {Email} користувачем {UserId}",
                dto.Email, requestingUserId);
            throw new InvalidOperationException("Email вже використовується");
        }

        if (await _userRepository.IsLoginExistsAsync(dto.Login))
        {
            _logger.LogWarning(
                "Спроба створення користувача з існуючим login: {Login} користувачем {UserId}",
                dto.Login, requestingUserId);
            throw new InvalidOperationException("Login вже використовується");
        }

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
        {
            _logger.LogWarning(
                "Спроба створення користувача з неіснуючим Agency ID: {AgencyId}",
                dto.AgencyId);
            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");
        }

        if (!agency.IsActive)
        {
            _logger.LogWarning(
                "Спроба створення користувача для неактивного Agency ID: {AgencyId}",
                dto.AgencyId);
            throw new InvalidOperationException("Неможливо створити користувача для неактивного Agency");
        }

        List<long> roleIdsToAssign;

        if (!dto.RoleId.Any())
        {
            var employeeRole = await _unitOfWork.Roles
                .GetQueryable()
                .FirstOrDefaultAsync(r => r.Name == SystemRoles.Employee && r.IsActive);

            if (employeeRole == null)
            {
                _logger.LogError("Роль Employee не знайдена в системі");
                throw new InvalidOperationException("Роль Employee не знайдена");
            }

            roleIdsToAssign = new List<long> { employeeRole.Id };
        }
        else
        {
            var roles = await _unitOfWork.Roles
                .GetQueryable()
                .Where(r => dto.RoleId.Contains(r.Id) && r.IsActive)
                .ToListAsync();

            if (roles.Count != dto.RoleId.Count)
            {
                var foundIds = roles.Select(r => r.Id);
                var missingIds = dto.RoleId.Except(foundIds);
                _logger.LogWarning(
                    "Ролі з ID {MissingIds} не знайдено або неактивні",
                    string.Join(", ", missingIds));
                throw new KeyNotFoundException(
                    $"Ролі з ID {string.Join(", ", missingIds)} не знайдено або неактивні");
            }

            roleIdsToAssign = dto.RoleId;
        }

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.IsActive = true;

        await _userRepository.AddAsync(user);

        foreach (var roleId in roleIdsToAssign)
        {
            user.UserRoles.Add(new UserRole
            {
                User = user,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        //АУДИТ - записуємо в БД 
        await _auditService.LogUserCreatedAsync(
            userId: user.Id,
            userName: user.Name,
            email: user.Email,
            agencyId: user.AgencyId,
            createdByUserId: requestingUserId,
            createdByUserName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        var createdUser = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateAsync(
        long id,
        UpdateUserDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Спроба оновлення неіснуючого користувача ID: {UserId}", id);
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");
        }

        // Email uniqueness
        if (await _userRepository.IsEmailExistsAsync(dto.Email, id))
        {
            _logger.LogWarning("Спроба зміни email на вже існуючий: {Email} для користувача ID: {UserId}", dto.Email,
                id);
            throw new InvalidOperationException("Email вже використовується іншим користувачем");
        }

        // Login uniqueness (тільки якщо передано)
        if (!string.IsNullOrWhiteSpace(dto.Login) && await _userRepository.IsLoginExistsAsync(dto.Login, id))
        {
            _logger.LogWarning("Спроба зміни логіну на вже існуючий: {Login} для користувача ID: {UserId}", dto.Login,
                id);
            throw new InvalidOperationException("Логін вже використовується іншим користувачем");
        }

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
            throw new KeyNotFoundException($"Агенцію з ID {dto.AgencyId} не знайдено");

        // Зберігаємо старі значення для аудиту
        var oldValues = new { user.Name, user.Email, user.Login, user.AgencyId };

        // Оновлюємо основні поля
        user.Name = dto.Name;
        user.Email = dto.Email;
        user.AgencyId = dto.AgencyId;

        if (!string.IsNullOrWhiteSpace(dto.Login))
            user.Login = dto.Login;

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;
        _userRepository.Update(user);

        // Оновлюємо ролі якщо передано
        if (dto.RoleIds != null && dto.RoleIds.Any())
        {
            // Видаляємо старі ролі
            user.UserRoles.Clear();

            // Додаємо нові
            foreach (var roleId in dto.RoleIds)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var newValues = new { user.Name, user.Email, user.Login, user.AgencyId };

        await _auditService.LogUserUpdatedAsync(
            userId: user.Id,
            userName: user.Name,
            oldValues: oldValues,
            newValues: newValues,
            updatedByUserId: requestingUserId,
            updatedByUserName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        var updatedUser = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task ActivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning(
                "Спроба активації неіснуючого користувача ID: {UserId}",
                id);
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");
        }

        if (user.IsActive)
        {
            _logger.LogInformation(
                "Користувач ID: {UserId} вже активний",
                id);
            throw new InvalidOperationException("Користувач вже активний");
        }

        user.IsActive = true;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ - записуємо в БД 
        await _auditService.LogUserActivatedAsync(
            userId: user.Id,
            userName: user.Name,
            activatedByUserId: requestingUserId,
            activatedByUserName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task DeactivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        // Завантажуємо з ролями для перевірки адміністратора
        var user = await _userRepository.GetByIdWithRolesAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Спроба деактивації неіснуючого користувача ID: {UserId}", id);
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");
        }

        if (!user.IsActive)
        {
            _logger.LogInformation("Користувач ID: {UserId} вже деактивований", id);
            throw new InvalidOperationException("Користувач вже деактивований");
        }

        // Захист останнього активного адміністратора
        var isAdmin = user.UserRoles.Any(ur => ur.Role.IsActive && ur.Role.Name == "Admin");
        if (isAdmin)
        {
            var admins = await _userRepository.GetUsersWithRoleAsync("Admin");
            var activeAdminsCount = admins.Count(u => u.IsActive);

            if (activeAdminsCount <= 1)
            {
                _logger.LogWarning(
                    "SECURITY: Спроба деактивації останнього активного адміністратора {UserId} ({UserName}) користувачем {RequestingUserId}",
                    id, user.Name, requestingUserId);
                throw new InvalidOperationException(
                    "Неможливо деактивувати останнього активного адміністратора системи.");
            }
        }

        user.IsActive = false;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogUserDeactivatedAsync(
            userId: user.Id,
            userName: user.Name,
            deactivatedByUserId: requestingUserId,
            deactivatedByUserName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task ChangePasswordAsync(
        long userId,
        ChangePasswordDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning(
                "Спроба зміни пароля для неіснуючого користувача ID: {UserId}",
                userId);
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Спроба зміни пароля для неактивного користувача ID: {UserId}",
                userId);
            throw new InvalidOperationException("Неможливо змінити пароль неактивного користувача");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning(
                "Невдала спроба зміни пароля: невірний поточний пароль для користувача ID: {UserId}",
                userId);
            throw new UnauthorizedAccessException("Поточний пароль невірний");
        }

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
        {
            _logger.LogWarning(
                "Спроба встановити однаковий пароль для користувача ID: {UserId}",
                userId);
            throw new InvalidOperationException("Новий пароль не може співпадати зі старим");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        //АУДИТ - записуємо в БД 
        bool isSelfChange = userId == requestingUserId;
        await _auditService.LogUserPasswordChangedAsync(
            userId: user.Id,
            userName: user.Name,
            changedByUserId: requestingUserId,
            changedByUserName: requestingUserName,
            isSelfChange: isSelfChange,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task<bool> IsEmailExistsAsync(string email, long? excludeUserId = null)
    {
        if (excludeUserId.HasValue)
            return await _userRepository.IsEmailExistsAsync(email, excludeUserId.Value);

        return await _userRepository.IsEmailExistsAsync(email);
    }

    public async Task<bool> IsLoginExistsAsync(string login, long? excludeUserId = null)
    {
        if (excludeUserId.HasValue)
            return await _userRepository.IsLoginExistsAsync(login, excludeUserId.Value);

        return await _userRepository.IsLoginExistsAsync(login);
    }
}