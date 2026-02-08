using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.RoleManagement;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;
    private readonly IAuditService _auditService;

    // Системні ролі які не можна видаляти або деактивувати
    private static readonly string[] SystemRoles = { "Admin", "Manager", "Employee", "Accountant" };

    public RoleService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RoleService> logger,
        IAuditService auditService)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<RoleDetailDto?> GetByIdAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return null;

        var dto = _mapper.Map<RoleDetailDto>(role);

        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        dto.UsersCount = users.Count();

        if (!string.IsNullOrWhiteSpace(role.Permissions))
        {
            try
            {
                dto.PermissionsList = JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();
            }
            catch
            {
                dto.PermissionsList = new List<string>();
            }
        }

        return dto;
    }

    public async Task<RoleDetailDto?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var role = await _roleRepository.GetByNameAsync(name);
        if (role == null)
            return null;

        return await GetByIdAsync(role.Id);
    }

    public async Task<IEnumerable<RoleListItemDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var dtos = new List<RoleListItemDto>();

        foreach (var role in roles)
        {
            var dto = _mapper.Map<RoleListItemDto>(role);
            var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
            dto.UsersCount = users.Count();
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<RoleListItemDto>> GetActiveRolesAsync()
    {
        var roles = await _roleRepository.GetActiveRolesAsync();
        var dtos = new List<RoleListItemDto>();

        foreach (var role in roles)
        {
            var dto = _mapper.Map<RoleListItemDto>(role);
            var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
            dto.UsersCount = users.Count();
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId)
    {
        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        var roles = await _roleRepository.GetUserRolesAsync(userId);
        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<IEnumerable<UserInRoleDto>> GetUsersInRoleAsync(long roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        return _mapper.Map<IEnumerable<UserInRoleDto>>(users);
    }

    public async Task<bool> UserHasRoleAsync(long userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
            return false;

        return await _roleRepository.UserHasRoleAsync(userId, roleName);
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(long roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        if (string.IsNullOrWhiteSpace(role.Permissions))
            return Enumerable.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();
        }
        catch (JsonException)
        {
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsRoleNameExistsAsync(string name, long? excludeRoleId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (excludeRoleId.HasValue)
            return await _roleRepository.IsRoleNameExistsAsync(name, excludeRoleId.Value);

        return await _roleRepository.IsRoleNameExistsAsync(name);
    }

    public async Task<bool> CanDeleteRoleAsync(long roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return false;

        if (IsSystemRole(role.Name))
            return false;

        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        return !users.Any();
    }

    public async Task<RoleDto> CreateAsync(
        CreateRoleDto dto,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        // Валидация
        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name))
        {
            _logger.LogWarning(
                "Спроба створення ролі з існуючою назвою: {Name} користувачем {UserId}",
                dto.Name, requestingUserId);
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");
        }

        if (IsSystemRole(dto.Name))
        {
            _logger.LogWarning(
                "Спроба створення системної ролі: {Name} користувачем {UserId}",
                dto.Name, requestingUserId);
            throw new InvalidOperationException($"Неможливо створити роль з системною назвою '{dto.Name}'");
        }

        // Создание роли
        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Permissions = dto.Permissions?.Trim(),
            IsActive = true
        };

        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль створена. Id: {Id}, Name: {Name}, створена користувачем {UserId}",
            role.Id, role.Name, requestingUserId);

        // 🔥 АУДИТ: Логируем создание роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var newValues = new
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    role.Permissions,
                    role.IsActive,
                    CreatedBy = requestingUser.Name
                };

                await _auditService.LogCreateAsync(
                    entityName: "Role",
                    entityId: role.Id,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту створення ролі {RoleId}", role.Id);
            // Не бросаем исключение - роль уже создана
        }

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(
        long id,
        UpdateRoleDto dto,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");
        }

        // Сохраняем старые значения для аудита
        var oldValues = new
        {
            role.Id,
            role.Name,
            role.Description,
            role.Permissions,
            role.IsActive
        };

        // Валидация изменения системных ролей
        if (IsSystemRole(role.Name) && role.Name != dto.Name)
        {
            _logger.LogWarning(
                "Спроба зміни назви системної ролі {OldName} на {NewName} користувачем {UserId}",
                role.Name, dto.Name, requestingUserId);
            throw new InvalidOperationException($"Неможливо змінити назву системної ролі '{role.Name}'");
        }

        if (!IsSystemRole(role.Name) && IsSystemRole(dto.Name))
        {
            _logger.LogWarning(
                "Спроба зміни назви ролі {OldName} на системну {NewName} користувачем {UserId}",
                role.Name, dto.Name, requestingUserId);
            throw new InvalidOperationException($"Неможливо змінити назву на системну '{dto.Name}'");
        }

        // Проверка уникальности нового имени
        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name, id))
        {
            _logger.LogWarning(
                "Спроба оновлення ролі {Id} з існуючою назвою: {Name} користувачем {UserId}",
                id, dto.Name, requestingUserId);
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");
        }

        // Обновление роли
        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();
        role.Permissions = dto.Permissions?.Trim();

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль оновлена. Id: {Id}, Name: {Name}, оновлена користувачем {UserId}",
            role.Id, role.Name, requestingUserId);

        // 🔥 АУДИТ: Логируем обновление роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var newValues = new
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    role.Permissions,
                    role.IsActive,
                    UpdatedBy = requestingUser.Name
                };

                await _auditService.LogUpdateAsync(
                    entityName: "Role",
                    entityId: role.Id,
                    oldValues: oldValues,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту оновлення ролі {RoleId}", role.Id);
            // Не бросаем исключение - роль уже обновлена
        }

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");
        }

        // Сохраняем значения для аудита
        var oldValues = new
        {
            role.Id,
            role.Name,
            role.Description,
            role.Permissions,
            role.IsActive
        };

        // Валидация удаления системных ролей
        if (IsSystemRole(role.Name))
        {
            _logger.LogWarning(
                "Спроба видалення системної ролі {Name} користувачем {UserId}",
                role.Name, requestingUserId);
            throw new InvalidOperationException($"Неможливо видалити системну роль '{role.Name}'");
        }

        // Проверка использования роли
        if (!await CanDeleteRoleAsync(id))
        {
            var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
            var usersCount = users.Count();

            _logger.LogWarning(
                "Спроба видалення ролі {Name} яка використовується ({UsersCount} користувачів) користувачем {UserId}",
                role.Name, usersCount, requestingUserId);

            throw new InvalidOperationException(
                $"Неможливо видалити роль '{role.Name}', оскільки вона призначена {usersCount} користувачам");
        }

        // Удаление роли
        _roleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль видалена. Id: {Id}, Name: {Name}, видалена користувачем {UserId}",
            role.Id, role.Name, requestingUserId);

        // 🔥 АУДИТ: Логируем удаление роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                await _auditService.LogDeleteAsync(
                    entityName: "Role",
                    entityId: role.Id,
                    oldValues: oldValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту видалення ролі {RoleId}", role.Id);
            // Не бросаем исключение - роль уже удалена
        }
    }

    public async Task ActivateAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");
        }

        if (role.IsActive)
        {
            throw new InvalidOperationException("Роль вже активна");
        }

        // Сохраняем старые значения для аудита
        var oldValues = new
        {
            role.Id,
            role.Name,
            IsActive = role.IsActive
        };

        // Активация роли
        role.IsActive = true;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль активована. Id: {Id}, Name: {Name}, активована користувачем {UserId}",
            role.Id, role.Name, requestingUserId);

        // 🔥 АУДИТ: Логируем активацию роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var newValues = new
                {
                    role.Id,
                    role.Name,
                    IsActive = role.IsActive,
                    ActivatedBy = requestingUser.Name
                };

                await _auditService.LogUpdateAsync(
                    entityName: "Role",
                    entityId: role.Id,
                    oldValues: oldValues,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту активації ролі {RoleId}", role.Id);
            // Не бросаем исключение - роль уже активирована
        }
    }

    public async Task DeactivateAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");
        }

        if (!role.IsActive)
        {
            throw new InvalidOperationException("Роль вже деактивована");
        }

        // Валидация деактивации системных ролей
        if (IsSystemRole(role.Name))
        {
            _logger.LogWarning(
                "Спроба деактивації системної ролі {Name} користувачем {UserId}",
                role.Name, requestingUserId);
            throw new InvalidOperationException($"Неможливо деактивувати системну роль '{role.Name}'");
        }

        // Проверка активных пользователей с этой ролью
        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        var activeUsers = users.Where(u => u.IsActive).ToList();

        if (activeUsers.Any())
        {
            _logger.LogWarning(
                "Спроба деактивації ролі {Name} яка призначена {ActiveUsersCount} активним користувачам користувачем {UserId}",
                role.Name, activeUsers.Count, requestingUserId);

            throw new InvalidOperationException(
                $"Неможливо деактивувати роль '{role.Name}'. " +
                $"Вона призначена {activeUsers.Count} активним користувачам. " +
                $"Спочатку деактивуйте користувачів або змініть їх ролі.");
        }

        // Сохраняем старые значения для аудита
        var oldValues = new
        {
            role.Id,
            role.Name,
            IsActive = role.IsActive
        };

        // Деактивация роли
        role.IsActive = false;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль деактивована. Id: {Id}, Name: {Name}, деактивована користувачем {UserId}",
            role.Id, role.Name, requestingUserId);

        // 🔥 АУДИТ: Логируем деактивацию роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var newValues = new
                {
                    role.Id,
                    role.Name,
                    IsActive = role.IsActive,
                    DeactivatedBy = requestingUser.Name
                };

                await _auditService.LogUpdateAsync(
                    entityName: "Role",
                    entityId: role.Id,
                    oldValues: oldValues,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту деактивації ролі {RoleId}", role.Id);
            // Не бросаем исключение - роль уже деактивирована
        }
    }

    public async Task AssignRoleToUserAsync(
        long userId,
        long roleId,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        if (!user.IsActive)
            throw new InvalidOperationException("Неможливо призначити роль неактивному користувачу");

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        if (!role.IsActive)
            throw new InvalidOperationException("Неможливо призначити неактивну роль");

        if (await _roleRepository.UserHasRoleAsync(userId, role.Name))
            throw new InvalidOperationException($"Роль '{role.Name}' вже призначена цьому користувачу");

        await _roleRepository.AssignRoleAsync(userId, roleId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль {RoleName} (ID: {RoleId}) призначена користувачу {UserId} користувачем {RequestingUserId}",
            role.Name, roleId, userId, requestingUserId);

        // 🔥 АУДИТ: Логируем назначение роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                await _auditService.LogRoleAssignedAsync(
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    targetUserId: userId,
                    targetUserName: user.Name,
                    roleId: roleId,
                    roleName: role.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту призначення ролі");
            // Не бросаем исключение - роль уже назначена
        }
    }

    public async Task RemoveRoleFromUserAsync(
        long userId,
        long roleId,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        if (!await _roleRepository.UserHasRoleAsync(userId, role.Name))
            throw new InvalidOperationException($"Роль '{role.Name}' не призначена цьому користувачу");

        var userRoles = await _roleRepository.GetUserRolesAsync(userId);
        if (userRoles.Count() == 1)
            throw new InvalidOperationException(
                "Неможливо видалити останню роль користувача. Користувач повинен мати хоча б одну роль");

        // Специальная проверка для Admin роли
        if (role.Name == "Admin")
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.IsActive)
            {
                var activeAdmins = await _userRepository.GetUsersWithRoleAsync("Admin");
                var activeAdminsCount = activeAdmins.Count(u => u.IsActive);

                if (activeAdminsCount <= 1)
                {
                    _logger.LogWarning(
                        "Спроба видалення ролі Admin у останнього активного адміністратора користувачем {RequestingUserId}",
                        requestingUserId);
                    throw new InvalidOperationException(
                        "Неможливо видалити роль Admin у останнього активного адміністратора");
                }
            }
        }

        await _roleRepository.RemoveRoleAsync(userId, roleId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Роль {RoleName} (ID: {RoleId}) видалена у користувача {UserId} користувачем {RequestingUserId}",
            role.Name, roleId, userId, requestingUserId);

        // 🔥 АУДИТ: Логируем удаление роли
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            var targetUser = await _userRepository.GetByIdAsync(userId);
            if (requestingUser != null && targetUser != null)
            {
                await _auditService.LogRoleRemovedAsync(
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    targetUserId: userId,
                    targetUserName: targetUser.Name,
                    roleId: roleId,
                    roleName: role.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту видалення ролі");
            // Не бросаем исключение - роль уже удалена
        }
    }

    public async Task ReplaceUserRolesAsync(
        long userId,
        IEnumerable<long> roleIds,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var roleIdsList = roleIds.ToList();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        if (!user.IsActive)
            throw new InvalidOperationException("Неможливо змінити ролі неактивного користувача");

        if (!roleIdsList.Any())
            throw new ArgumentException("Необхідно передати хоча б одну роль");

        if (roleIdsList.Distinct().Count() != roleIdsList.Count)
            throw new ArgumentException("Список ролей містить дублікати");

        // Проверка существования и активности всех ролей
        var roles = new List<Role>();
        foreach (var roleId in roleIdsList)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

            if (!role.IsActive)
                throw new InvalidOperationException($"Роль '{role.Name}' (ID: {roleId}) неактивна");

            roles.Add(role);
        }

        // Получаем текущие роли для сравнения
        var currentRoles = await _roleRepository.GetUserRolesAsync(userId);
        var hasAdminNow = currentRoles.Any(r => r.Name == "Admin");
        var willHaveAdmin = roles.Any(r => r.Name == "Admin");

        // Проверка на удаление Admin роли у последнего администратора
        if (hasAdminNow && !willHaveAdmin && user.IsActive)
        {
            var activeAdmins = await _userRepository.GetUsersWithRoleAsync("Admin");
            var activeAdminsCount = activeAdmins.Count(u => u.IsActive);

            if (activeAdminsCount <= 1)
            {
                _logger.LogWarning(
                    "Спроба видалення ролі Admin у останнього активного адміністратора користувачем {RequestingUserId}",
                    requestingUserId);
                throw new InvalidOperationException(
                    "Неможливо видалити роль Admin у останнього активного адміністратора");
            }
        }

        // Заменяем роли
        await _roleRepository.ReplaceUserRolesAsync(userId, roleIdsList);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Ролі користувача {UserId} замінені користувачем {RequestingUserId}. Нові ролі: {NewRoles}",
            userId, requestingUserId, string.Join(", ", roles.Select(r => r.Name)));

        // 🔥 АУДИТ: Логируем замену ролей
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var oldRoleNames = string.Join(", ", currentRoles.Select(r => r.Name));
                var newRoleNames = string.Join(", ", roles.Select(r => r.Name));

                var oldValues = new
                {
                    UserId = userId,
                    UserName = user.Name,
                    RoleIds = currentRoles.Select(r => r.Id).ToList(),
                    RoleNames = oldRoleNames
                };
                var newValues = new
                {
                    UserId = userId,
                    UserName = user.Name,
                    RoleIds = roleIdsList,
                    RoleNames = newRoleNames,
                    UpdatedBy = requestingUser.Name
                };

                await _auditService.LogUpdateAsync(
                    entityName: "UserRoles",
                    entityId: userId,
                    oldValues: oldValues,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту заміни ролей користувача {UserId}", userId);
            // Не бросаем исключение - роли уже заменены
        }
    }

    public async Task UpdateRolePermissionsAsync(
        long roleId,
        IEnumerable<string> permissions,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        var permissionsList = permissions.ToList();

        // Сохраняем старые permissions для аудита
        var oldPermissions = string.IsNullOrWhiteSpace(role.Permissions)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();

        // КРИТИЧЕСКОЕ ПРЕДУПРЕЖДЕНИЕ при изменении системных ролей
        if (IsSystemRole(role.Name))
        {
            _logger.LogWarning(
                "КРИТИЧНА ДІЯ: Оновлення permissions системної ролі {RoleName} (ID: {RoleId}) користувачем {RequestingUserId}",
                role.Name, roleId, requestingUserId);
        }

        // Обновление permissions
        role.Permissions = permissionsList.Any()
            ? JsonSerializer.Serialize(permissionsList)
            : null;

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Permissions ролі {RoleName} (ID: {RoleId}) оновлені користувачем {RequestingUserId}",
            role.Name, roleId, requestingUserId);

        // 🔥 АУДИТ: Логируем обновление permissions
        try
        {
            var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
            if (requestingUser != null)
            {
                var oldValues = new
                {
                    RoleId = roleId,
                    RoleName = role.Name,
                    Permissions = oldPermissions
                };

                var newValues = new
                {
                    RoleId = roleId,
                    RoleName = role.Name,
                    Permissions = permissionsList,
                    UpdatedBy = requestingUser.Name
                };

                await _auditService.LogUpdateAsync(
                    entityName: "RolePermissions",
                    entityId: roleId,
                    oldValues: oldValues,
                    newValues: newValues,
                    userId: requestingUserId,
                    userName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні аудиту оновлення permissions ролі {RoleId}", roleId);
            // Не бросаем исключение - permissions уже обновлены
        }
    }

    private bool IsSystemRole(string roleName)
    {
        return SystemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}