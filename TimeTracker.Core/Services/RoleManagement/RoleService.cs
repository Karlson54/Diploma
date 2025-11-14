using System.Text.Json;
using AutoMapper;
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

    public RoleService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Назва ролі не може бути порожньою");

        if (dto.Name.Length < 3)
            throw new ArgumentException("Назва ролі має бути мінімум 3 символи");

        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name))
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");

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
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Назва ролі не може бути порожньою");

        if (dto.Name.Length < 3)
            throw new ArgumentException("Назва ролі має бути мінімум 3 символи");

        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new KeyNotFoundException($"Роль з ID {id} не знайдено");

        if (IsSystemRole(role.Name) && role.Name != dto.Name)
            throw new InvalidOperationException("Неможливо змінити назву системної ролі");

        if (await _roleRepository.IsRoleNameExistsAsync(dto.Name, id))
            throw new InvalidOperationException($"Роль з назвою '{dto.Name}' вже існує");

        role.Name = dto.Name.Trim();
        role.Description = dto.Description?.Trim();
        role.Permissions = dto.Permissions?.Trim();
        role.IsActive = dto.IsActive;

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
            throw new InvalidOperationException("Неможливо видалити системну роль");

        if (!await CanDeleteRoleAsync(id))
            throw new InvalidOperationException("Неможливо видалити роль, яка призначена користувачам");

        _roleRepository.Delete(role);
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
            throw new InvalidOperationException("Неможливо видалити останню роль користувача");

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

        foreach (var roleId in roleIdsList)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Роль з ID {roleId} не знайдено");

            if (!role.IsActive)
                throw new InvalidOperationException($"Роль з ID {roleId} неактивна");
        }

        if (roleIdsList.Distinct().Count() != roleIdsList.Count)
            throw new ArgumentException("Список ролей містить дублікати");

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
        
        foreach (var permission in permissionsList)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("Permission не може бути порожнім");
        }

        if (permissionsList.Distinct().Count() != permissionsList.Count)
            throw new ArgumentException("Список permissions містить дублікати");

        role.Permissions = JsonSerializer.Serialize(permissionsList);
        
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();
    }

    private bool IsSystemRole(string roleName)
    {
        var systemRoles = new[] { "Admin", "Manager", "Employee", "Accountant" };
        return systemRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}