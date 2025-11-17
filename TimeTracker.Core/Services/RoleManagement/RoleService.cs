using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Roles;
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

    // Системні ролі які не можна видаляти або деактивувати
    private static readonly string[] SystemRoles = { "Admin", "Manager", "Employee", "Accountant" };

    public RoleService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name))
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");

        if (IsSystemRole(dto.Name))
            throw new InvalidOperationException($"Неможливо створити роль з системною назвою '{dto.Name}'");

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Permissions = dto.Permissions?.Trim(),
            IsActive = true
        };

        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(long id, UpdateRoleDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");

        if (IsSystemRole(role.Name) && role.Name != dto.Name)
            throw new InvalidOperationException($"Неможливо змінити назву системної ролі '{role.Name}'");

        if (!IsSystemRole(role.Name) && IsSystemRole(dto.Name))
            throw new InvalidOperationException($"Неможливо змінити назву на системну '{dto.Name}'");

        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");

        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();
        role.Permissions = dto.Permissions?.Trim();

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");

        if (IsSystemRole(role.Name))
            throw new InvalidOperationException($"Неможливо видалити системну роль '{role.Name}'");

        if (!await CanDeleteRoleAsync(id))
            throw new InvalidOperationException("Неможливо видалити роль, яка призначена користувачам");

        _roleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ActivateAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");

        if (role.IsActive)
            throw new InvalidOperationException("Роль вже активна");

        role.IsActive = true;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");

        if (!role.IsActive)
            throw new InvalidOperationException("Роль вже деактивована");

        if (IsSystemRole(role.Name))
            throw new InvalidOperationException($"Неможливо деактивувати системну роль '{role.Name}'");

        var users = await _roleRepository.GetUsersInRoleAsync(role.Name);
        var activeUsers = users.Where(u => u.IsActive).ToList();
        
        if (activeUsers.Any())
            throw new InvalidOperationException(
                $"Неможливо деактивувати роль. Вона призначена {activeUsers.Count} активним користувачам");

        role.IsActive = false;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();
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

    public async Task AssignRoleToUserAsync(long userId, long roleId)
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
    }

    public async Task RemoveRoleFromUserAsync(long userId, long roleId)
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
            throw new InvalidOperationException("Неможливо видалити останню роль користувача. Користувач повинен мати хоча б одну роль");

        if (role.Name == "Admin")
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.IsActive)
            {
                var activeAdmins = await _userRepository.GetUsersWithRoleAsync("Admin");
                var activeAdminsCount = activeAdmins.Count(u => u.IsActive);
                
                if (activeAdminsCount <= 1)
                    throw new InvalidOperationException("Неможливо видалити роль Admin у останнього активного адміністратора");
            }
        }

        await _roleRepository.RemoveRoleAsync(userId, roleId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReplaceUserRolesAsync(long userId, IEnumerable<long> roleIds)
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

        var currentRoles = await _roleRepository.GetUserRolesAsync(userId);
        var hasAdminNow = currentRoles.Any(r => r.Name == "Admin");
        var willHaveAdmin = roles.Any(r => r.Name == "Admin");
        
        if (hasAdminNow && !willHaveAdmin && user.IsActive)
        {
            var activeAdmins = await _userRepository.GetUsersWithRoleAsync("Admin");
            var activeAdminsCount = activeAdmins.Count(u => u.IsActive);
            
            if (activeAdminsCount <= 1)
                throw new InvalidOperationException("Неможливо видалити роль Admin у останнього активного адміністратора");
        }

        await _roleRepository.ReplaceUserRolesAsync(userId, roleIdsList);
        await _unitOfWork.SaveChangesAsync();
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

    private bool IsSystemRole(string roleName)
    {
        return SystemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
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

    public async Task UpdateRolePermissionsAsync(long roleId, IEnumerable<string> permissions)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

        var permissionsList = permissions.ToList();

        if (IsSystemRole(role.Name))
        {
            var oldPermissions = string.IsNullOrWhiteSpace(role.Permissions)
                ? "[]"
                : role.Permissions;
    
            var newPermissions = permissionsList.Any()
                ? JsonSerializer.Serialize(permissionsList)
                : "[]";

            _logger.LogWarning(
                "КРИТИЧНА ДІЯ: Оновлення permissions системної ролі {RoleName} (ID: {RoleId}). " +
                "Старі: {OldPermissions}, Нові: {NewPermissions}",
                role.Name, roleId, oldPermissions, newPermissions);
        }

        role.Permissions = permissionsList.Any() 
            ? JsonSerializer.Serialize(permissionsList) 
            : null;
        
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();
    }
}