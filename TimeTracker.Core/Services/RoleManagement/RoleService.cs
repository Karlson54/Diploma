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

    private static readonly string[] SystemRoles = Common.SystemRoles.All;

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
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(
                "Невалідний токен при створенні ролі. UserId: {UserId}",
                requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name))
        {
            var errorMsg = $"Роль з назвою '{dto.Name}' вже існує";

            _logger.LogWarning(
                "Спроба створення ролі з існуючою назвою: {Name} користувачем {UserId} ({UserName})",
                dto.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "CreateRole",
                entityName: "Role",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { dto.Name, dto.Description });

            throw new InvalidOperationException(errorMsg);
        }

        if (IsSystemRole(dto.Name))
        {
            var errorMsg = $"Неможливо створити роль з системною назвою '{dto.Name}'";

            _logger.LogWarning(
                "SECURITY:Спроба створення системної ролі: {Name} користувачем {UserId} ({UserName})",
                dto.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "CreateRole",
                entityName: "Role",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { dto.Name, Reason = "SystemRole" });

            throw new InvalidOperationException(errorMsg);
        }

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Permissions = dto.Permissions?.Trim(),
            IsActive = true
        };

        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

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

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(
        long id,
        UpdateRoleDto dto,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning("Невалідний токен при оновленні ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {id} не знайдено";

            _logger.LogWarning(
                "Спроба оновлення неіснуючої ролі {RoleId} користувачем {UserId} ({UserName})",
                id, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "UpdateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { AttemptedName = dto.Name });

            throw new KeyNotFoundException(errorMsg);
        }

        var oldValues = new
        {
            role.Id,
            role.Name,
            role.Description,
            role.Permissions,
            role.IsActive
        };

        if (IsSystemRole(role.Name) && role.Name != dto.Name)
        {
            var errorMsg = $"Неможливо змінити назву системної ролі '{role.Name}'";

            _logger.LogWarning(
                " SECURITY:Спроба зміни назви системної ролі {OldName} на {NewName} користувачем {UserId} ({UserName})",
                role.Name, dto.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "UpdateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { OldName = role.Name, NewName = dto.Name, Reason = "SystemRole" });

            throw new InvalidOperationException(errorMsg);
        }

        if (!IsSystemRole(role.Name) && IsSystemRole(dto.Name))
        {
            var errorMsg = $"Неможливо змінити назву на системну '{dto.Name}'";

            _logger.LogWarning(
                " SECURITY:Спроба зміни назви ролі {OldName} на системну {NewName} користувачем {UserId} ({UserName})",
                role.Name, dto.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "UpdateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { OldName = role.Name, NewName = dto.Name, Reason = "SystemRole" });

            throw new InvalidOperationException(errorMsg);
        }

        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name, id))
        {
            var errorMsg = $"Роль з назвою '{dto.Name}' вже існує";

            _logger.LogWarning(
                "Спроба оновлення ролі {Id} з існуючою назвою: {Name} користувачем {UserId} ({UserName})",
                id, dto.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "UpdateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { OldName = role.Name, NewName = dto.Name });

            throw new InvalidOperationException(errorMsg);
        }

        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();
        role.Permissions = dto.Permissions?.Trim();

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

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

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при видаленні ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {id} не знайдено";

            _logger.LogWarning(
                "Спроба видалення неіснуючої ролі {RoleId} користувачем {UserId} ({UserName})",
                id, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeleteRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg);

            throw new KeyNotFoundException(errorMsg);
        }

        var oldValues = new
        {
            role.Id,
            role.Name,
            role.Description,
            role.Permissions,
            role.IsActive
        };

        if (IsSystemRole(role.Name))
        {
            var errorMsg = $"Неможливо видалити системну роль '{role.Name}'";

            _logger.LogWarning(
                " SECURITY:Спроба видалення системної ролі {Name} користувачем {UserId} ({UserName})",
                role.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeleteRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { RoleName = role.Name, Reason = "SystemRole" });

            throw new InvalidOperationException(errorMsg);
        }

        if (!await CanDeleteRoleAsync(id))
        {
            var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
            var usersCount = users.Count();
            var errorMsg = $"Неможливо видалити роль '{role.Name}', оскільки вона призначена {usersCount} користувачам";

            _logger.LogWarning(
                "Спроба видалення ролі {Name} яка використовується ({UsersCount} користувачів) користувачем {UserId} ({UserName})",
                role.Name, usersCount, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeleteRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { RoleName = role.Name, UsersCount = usersCount });

            throw new InvalidOperationException(errorMsg);
        }

        _roleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogDeleteAsync(
            entityName: "Role",
            entityId: role.Id,
            oldValues: oldValues,
            userId: requestingUserId,
            userName: requestingUser.Name,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task ActivateAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при активації ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {id} не знайдено";

            _logger.LogWarning(
                "Спроба активації неіснуючої ролі {RoleId} користувачем {UserId} ({UserName})",
                id, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "ActivateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg);

            throw new KeyNotFoundException(errorMsg);
        }

        if (role.IsActive)
        {
            _logger.LogWarning(
                "Спроба активації вже активної ролі {Name} (ID: {RoleId}) користувачем {UserId} ({UserName})",
                role.Name, id, requestingUserId, requestingUser.Name);

            throw new InvalidOperationException("Роль вже активна");
        }

        var oldValues = new
        {
            role.Id,
            role.Name,
            IsActive = role.IsActive
        };

        role.IsActive = true;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task DeactivateAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при деактивації ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {id} не знайдено";

            _logger.LogWarning(
                "Спроба деактивації неіснуючої ролі {RoleId} користувачем {UserId} ({UserName})",
                id, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeactivateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg);

            throw new KeyNotFoundException(errorMsg);
        }

        if (!role.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже неактивної ролі {Name} (ID: {RoleId}) користувачем {UserId} ({UserName})",
                role.Name, id, requestingUserId, requestingUser.Name);

            throw new InvalidOperationException("Роль вже деактивована");
        }

        if (IsSystemRole(role.Name))
        {
            var errorMsg = $"Неможливо деактивувати системну роль '{role.Name}'";

            _logger.LogWarning(
                " SECURITY:Спроба деактивації системної ролі {Name} користувачем {UserId} ({UserName})",
                role.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeactivateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { RoleName = role.Name, Reason = "SystemRole" });

            throw new InvalidOperationException(errorMsg);
        }

        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        var activeUsers = users.Where(u => u.IsActive).ToList();

        if (activeUsers.Any())
        {
            var errorMsg = $"Неможливо деактивувати роль '{role.Name}'. " +
                           $"Вона призначена {activeUsers.Count} активним користувачам. " +
                           $"Спочатку деактивуйте користувачів або змініть їх ролі.";

            _logger.LogWarning(
                "Спроба деактивації ролі {Name} яка призначена {ActiveUsersCount} активним користувачам користувачем {UserId} ({UserName})",
                role.Name, activeUsers.Count, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "DeactivateRole",
                entityName: "Role",
                entityId: id,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { RoleName = role.Name, ActiveUsersCount = activeUsers.Count });

            throw new InvalidOperationException(errorMsg);
        }

        var oldValues = new
        {
            role.Id,
            role.Name,
            IsActive = role.IsActive
        };

        role.IsActive = false;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task AssignRoleToUserAsync(
        long userId,
        long roleId,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при призначенні ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            var errorMsg = $"Користувача з ID {userId} не знайдено";

            _logger.LogWarning(
                "Спроба призначення ролі неіснуючому користувачу {UserId} користувачем {RequestingUserId} ({UserName})",
                userId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "AssignRole",
                entityName: "UserRole",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { UserId = userId, RoleId = roleId });

            throw new KeyNotFoundException(errorMsg);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Спроба призначення ролі неактивному користувачу {UserId} ({UserName}) користувачем {RequestingUserId}",
                userId, user.Name, requestingUserId);

            throw new InvalidOperationException("Неможливо призначити роль неактивному користувачу");
        }

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {roleId} не знайдено";

            _logger.LogWarning(
                "Спроба призначення неіснуючої ролі {RoleId} користувачу {UserId} користувачем {RequestingUserId} ({UserName})",
                roleId, userId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "AssignRole",
                entityName: "UserRole",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { UserId = userId, UserName = user.Name, RoleId = roleId });

            throw new KeyNotFoundException(errorMsg);
        }

        if (!role.IsActive)
        {
            _logger.LogWarning(
                "Спроба призначення неактивної ролі {RoleName} (ID: {RoleId}) користувачу {UserId} користувачем {RequestingUserId} ({UserName})",
                role.Name, roleId, userId, requestingUserId, requestingUser.Name);

            throw new InvalidOperationException("Неможливо призначити неактивну роль");
        }

        if (await _roleRepository.UserHasRoleAsync(userId, role.Name))
        {
            _logger.LogWarning(
                "Спроба повторного призначення ролі {RoleName} користувачу {UserId} ({UserName}) користувачем {RequestingUserId}",
                role.Name, userId, user.Name, requestingUserId);

            throw new InvalidOperationException($"Роль '{role.Name}' вже призначена цьому користувачу");
        }

        await _roleRepository.AssignRoleAsync(userId, roleId);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task RemoveRoleFromUserAsync(
        long userId,
        long roleId,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при видаленні ролі. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            var errorMsg = $"Користувача з ID {userId} не знайдено";

            _logger.LogWarning(
                "Спроба видалення ролі у неіснуючого користувача {UserId} користувачем {RequestingUserId} ({UserName})",
                userId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "RemoveRole",
                entityName: "UserRole",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { UserId = userId, RoleId = roleId });

            throw new KeyNotFoundException(errorMsg);
        }

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {roleId} не знайдено";

            _logger.LogWarning(
                "Спроба видалення неіснуючої ролі {RoleId} у користувача {UserId} користувачем {RequestingUserId} ({UserName})",
                roleId, userId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "RemoveRole",
                entityName: "UserRole",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { UserId = userId, UserName = user.Name, RoleId = roleId });

            throw new KeyNotFoundException(errorMsg);
        }

        if (!await _roleRepository.UserHasRoleAsync(userId, role.Name))
        {
            _logger.LogWarning(
                "Спроба видалення не призначеної ролі {RoleName} у користувача {UserId} ({UserName}) користувачем {RequestingUserId}",
                role.Name, userId, user.Name, requestingUserId);

            throw new InvalidOperationException($"Роль '{role.Name}' не призначена цьому користувачу");
        }

        var userRoles = await _roleRepository.GetUserRolesAsync(userId);
        if (userRoles.Count() == 1)
        {
            var errorMsg = "Неможливо видалити останню роль користувача. Користувач повинен мати хоча б одну роль";

            _logger.LogWarning(
                "Спроба видалення останньої ролі {RoleName} у користувача {UserId} ({UserName}) користувачем {RequestingUserId} ({UserName})",
                role.Name, userId, user.Name, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "RemoveRole",
                entityName: "UserRole",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new
                {
                    UserId = userId, UserName = user.Name, RoleId = roleId, RoleName = role.Name,
                    RemainingRolesCount = 1
                });

            throw new InvalidOperationException(errorMsg);
        }

        if (role.Name == "SuperAdmin" && user.IsActive)
        {
            var activeAdmins = await _userRepository.GetUsersWithRoleAsync("Admin");
            var activeAdminsCount = activeAdmins.Count(u => u.IsActive);

            if (activeAdminsCount <= 1)
            {
                var errorMsg = "Неможливо видалити роль Admin у останнього активного адміністратора";

                _logger.LogWarning(
                    " SECURITY:Спроба видалення ролі Admin у останнього активного адміністратора {UserId} ({UserName}) користувачем {RequestingUserId} ({RequestingUserName})",
                    userId, user.Name, requestingUserId, requestingUser.Name);

                await LogFailedOperationAsync(
                    action: "RemoveRole",
                    entityName: "UserRole",
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    errorMessage: errorMsg,
                    additionalData: new
                    {
                        UserId = userId, UserName = user.Name, RoleId = roleId, RoleName = role.Name,
                        ActiveAdminsCount = activeAdminsCount, Reason = "LastAdmin"
                    });

                throw new InvalidOperationException(errorMsg);
            }
        }

        await _roleRepository.RemoveRoleAsync(userId, roleId);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogRoleRemovedAsync(
            userId: requestingUserId,
            userName: requestingUser.Name,
            targetUserId: userId,
            targetUserName: user.Name,
            roleId: roleId,
            roleName: role.Name,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task ReplaceUserRolesAsync(
        long userId,
        IEnumerable<long> roleIds,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var roleIdsList = roleIds.ToList();
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при заміні ролей. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            var errorMsg = $"Користувача з ID {userId} не знайдено";

            _logger.LogWarning(
                "Спроба заміни ролей неіснуючого користувача {UserId} користувачем {RequestingUserId} ({UserName})",
                userId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "ReplaceUserRoles",
                entityName: "UserRoles",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { UserId = userId, NewRoleIds = roleIdsList });

            throw new KeyNotFoundException(errorMsg);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Спроба заміни ролей неактивного користувача {UserId} ({UserName}) користувачем {RequestingUserId}",
                userId, user.Name, requestingUserId);

            throw new InvalidOperationException("Неможливо змінити ролі неактивного користувача");
        }

        if (!roleIdsList.Any())
        {
            _logger.LogWarning(
                "Спроба заміни ролей порожнім списком для користувача {UserId} ({UserName}) користувачем {RequestingUserId} ({RequestingUserName})",
                userId, user.Name, requestingUserId, requestingUser.Name);

            throw new ArgumentException("Необхідно передати хоча б одну роль");
        }

        if (roleIdsList.Distinct().Count() != roleIdsList.Count)
        {
            _logger.LogWarning(
                "Спроба заміни ролей зі дублікатами для користувача {UserId} ({UserName}) користувачем {RequestingUserId} ({RequestingUserName})",
                userId, user.Name, requestingUserId, requestingUser.Name);

            throw new ArgumentException("Список ролей містить дублікати");
        }

        var roles = new List<Role>();
        foreach (var roleId in roleIdsList)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);

            if (role == null)
            {
                var errorMsg = $"Роль з ID {roleId} не знайдено";

                _logger.LogWarning(
                    "Спроба заміни ролей з неіснуючою роллю {RoleId} для користувача {UserId} ({UserName}) користувачем {RequestingUserId} ({RequestingUserName})",
                    roleId, userId, user.Name, requestingUserId, requestingUser.Name);

                await LogFailedOperationAsync(
                    action: "ReplaceUserRoles",
                    entityName: "UserRoles",
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    errorMessage: errorMsg,
                    additionalData: new
                        { UserId = userId, UserName = user.Name, NewRoleIds = roleIdsList, MissingRoleId = roleId });

                throw new KeyNotFoundException(errorMsg);
            }

            if (!role.IsActive)
            {
                var errorMsg = $"Роль '{role.Name}' (ID: {roleId}) неактивна";

                _logger.LogWarning(
                    "Спроба заміни ролей з неактивною роллю {RoleName} (ID: {RoleId}) для користувача {UserId} ({UserName}) користувачем {RequestingUserId} ({RequestingUserName})",
                    role.Name, roleId, userId, user.Name, requestingUserId, requestingUser.Name);

                await LogFailedOperationAsync(
                    action: "ReplaceUserRoles",
                    entityName: "UserRoles",
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    errorMessage: errorMsg,
                    additionalData: new
                    {
                        UserId = userId, UserName = user.Name, NewRoleIds = roleIdsList, InactiveRoleId = roleId,
                        InactiveRoleName = role.Name
                    });

                throw new InvalidOperationException(errorMsg);
            }

            roles.Add(role);
        }

        var currentRoles = await _roleRepository.GetUserRolesAsync(userId);
        var hasSuperAdminNow = currentRoles.Any(r => r.Name == "SuperAdmin");
        var willHaveSuperAdmin = roles.Any(r => r.Name == "SuperAdmin");

        if (hasSuperAdminNow && !willHaveSuperAdmin && user.IsActive)
        {
            var activeSuperAdmin = await _userRepository.GetUsersWithRoleAsync("SuperAdmin");
            var activeSuperAdminCount = activeSuperAdmin.Count(u => u.IsActive);

            if (activeSuperAdminCount <= 1)
            {
                var errorMsg = "Неможливо видалити роль Admin у останнього активного адміністратора";

                _logger.LogWarning(
                    "SECURITY:Спроба видалення ролі Admin у останнього активного адміністратора {UserId} ({UserName}) через заміну ролей користувачем {RequestingUserId} ({RequestingUserName})",
                    userId, user.Name, requestingUserId, requestingUser.Name);

                await LogFailedOperationAsync(
                    action: "ReplaceUserRoles",
                    entityName: "UserRoles",
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent,
                    errorMessage: errorMsg,
                    additionalData: new
                    {
                        UserId = userId,
                        UserName = user.Name,
                        OldRoles = string.Join(", ", currentRoles.Select(r => r.Name)),
                        NewRoles = string.Join(", ", roles.Select(r => r.Name)),
                        ActiveAdminsCount = activeSuperAdminCount,
                        Reason = "LastAdmin"
                    });

                throw new InvalidOperationException(errorMsg);
            }
        }

        await _roleRepository.ReplaceUserRolesAsync(userId, roleIdsList);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task UpdateRolePermissionsAsync(
        long roleId,
        IEnumerable<string> permissions,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var permissionsList = permissions.ToList();
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);

        if (requestingUser == null)
        {
            _logger.LogWarning(" Невалідний токен при оновленні permissions. UserId: {UserId}", requestingUserId);
            throw new UnauthorizedAccessException("Невалідний токен користувача");
        }

        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            var errorMsg = $"Роль з ID {roleId} не знайдено";

            _logger.LogWarning(
                "Спроба оновлення permissions неіснуючої ролі {RoleId} користувачем {UserId} ({UserName})",
                roleId, requestingUserId, requestingUser.Name);

            await LogFailedOperationAsync(
                action: "UpdateRolePermissions",
                entityName: "RolePermissions",
                entityId: roleId,
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent,
                errorMessage: errorMsg,
                additionalData: new { RoleId = roleId, NewPermissions = permissionsList });

            throw new KeyNotFoundException(errorMsg);
        }

        var oldPermissions = string.IsNullOrWhiteSpace(role.Permissions)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();

        if (IsSystemRole(role.Name))
        {
            _logger.LogWarning(
                "Спроба CRITICAL: Оновлення permissions системної ролі {RoleName} (ID: {RoleId}) користувачем {RequestingUserId} ({RequestingUserName})",
                role.Name, roleId, requestingUserId, requestingUser.Name);
        }

        role.Permissions = permissionsList.Any()
            ? JsonSerializer.Serialize(permissionsList)
            : null;

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

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

    private bool IsSystemRole(string roleName)
    {
        return Common.SystemRoles.IsSystemRole(roleName);
    }

    private async Task LogFailedOperationAsync(
        string action,
        string entityName,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent,
        string errorMessage,
        object? additionalData = null,
        long? entityId = null)
    {
        try
        {
            var failureData = new
            {
                Action = action,
                ErrorMessage = errorMessage,
                AdditionalData = additionalData
            };

            await _auditService.LogCreateAsync(
                entityName: $"{entityName}Failure",
                entityId: entityId ?? 0,
                newValues: failureData,
                userId: requestingUserId,
                userName: requestingUserName,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при логуванні неуспішної операції {Action} для {EntityName}", action,
                entityName);
        }
    }
}