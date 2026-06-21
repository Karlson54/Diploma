using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.AdminPermissions;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.AdminPermissions;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.AdminPermissions;

public class AdminPermissionService : IAdminPermissionService
{
    private readonly IAdminPermissionRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminPermissionService> _logger;

    public AdminPermissionService(
        IAdminPermissionRepository repository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<AdminPermissionService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminPermissionsForUserDto> GetByUserIdAsync(long userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        var permissions = await _repository.GetByUserIdAsync(userId);

        return new AdminPermissionsForUserDto
        {
            UserId = userId,
            UserName = user.Name,
            Permissions = permissions.Select(p => new AdminPermissionDto
            {
                AgencyId = p.AgencyId,
                AgencyName = p.Agency.Name,
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department.Name,
            }).ToList()
        };
    }

    public async Task SetPermissionsAsync(long userId, SetAdminPermissionsDto dto, long requestingUserId)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId)
            ?? throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        // Только Admin может получать разрешения
        var isAdmin = user.UserRoles.Any(ur => ur.Role.IsActive && ur.Role.Name == Common.SystemRoles.Admin);
        if (!isAdmin)
            throw new InvalidOperationException("Дозволи можна призначати тільки користувачам з роллю Admin");

        // Валидируем что агенция и отдел существуют и отдел принадлежит агенции
        foreach (var item in dto.Permissions)
        {
            var agency = await _unitOfWork.Agencies.GetByIdAsync(item.AgencyId)
                ?? throw new KeyNotFoundException($"Агенцію з ID {item.AgencyId} не знайдено");

            var department = await _unitOfWork.Departments.GetByIdAsync(item.DepartmentId)
                ?? throw new KeyNotFoundException($"Відділ з ID {item.DepartmentId} не знайдено");

            if (department.AgencyId != item.AgencyId)
                throw new InvalidOperationException(
                    $"Відділ '{department.Name}' не належить до агенції '{agency.Name}'");
        }

        // Полная перезапись: удаляем старые, добавляем новые
        await _repository.DeleteAllByUserIdAsync(userId);

        foreach (var item in dto.Permissions)
        {
            await _repository.AddAsync(new AdminAgencyPermission
            {
                UserId = userId,
                AgencyId = item.AgencyId,
                DepartmentId = item.DepartmentId,
                CreatedByUserId = requestingUserId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "SuperAdmin {RequestingUserId} встановив {Count} дозволів для Admin {UserId}",
            requestingUserId, dto.Permissions.Count, userId);
    }

    public async Task ClearPermissionsAsync(long userId, long requestingUserId)
    {
        await _repository.DeleteAllByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "SuperAdmin {RequestingUserId} очистив усі дозволи Admin {UserId}",
            requestingUserId, userId);
    }

    public async Task<bool> HasAccessAsync(long userId, long agencyId, long departmentId)
    {
        return await _repository.ExistsAsync(userId, agencyId, departmentId);
    }

    public async Task<IEnumerable<(long AgencyId, long DepartmentId)>> GetAllowedScopesAsync(long userId)
    {
        var permissions = await _repository.GetByUserIdAsync(userId);
        return permissions.Select(p => (p.AgencyId, p.DepartmentId));
    }
}