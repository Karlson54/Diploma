using TimeTracker.Core.DTOs.AdminPermissions;

namespace TimeTracker.Core.Services.AdminPermissions;

public interface IAdminPermissionService
{
    /// <summary>Получить все разрешения конкретного Admin-а</summary>
    Task<AdminPermissionsForUserDto> GetByUserIdAsync(long userId);

    /// <summary>Заменить все разрешения Admin-а (полная перезапись)</summary>
    Task SetPermissionsAsync(long userId, SetAdminPermissionsDto dto, long requestingUserId);

    /// <summary>Удалить все разрешения Admin-а</summary>
    Task ClearPermissionsAsync(long userId, long requestingUserId);

    /// <summary>Проверить, имеет ли Admin доступ к конкретному отделу</summary>
    Task<bool> HasAccessAsync(long userId, long agencyId, long departmentId);

    /// <summary>Получить список разрешённых departmentId для Admin-а (для фильтрации отчётов)</summary>
    Task<IEnumerable<(long AgencyId, long DepartmentId)>> GetAllowedScopesAsync(long userId);
}