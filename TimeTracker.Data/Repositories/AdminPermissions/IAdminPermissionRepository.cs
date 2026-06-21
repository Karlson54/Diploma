using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Repositories.AdminPermissions;

public interface IAdminPermissionRepository
{
    Task<IEnumerable<AdminAgencyPermission>> GetByUserIdAsync(long userId);
    Task<bool> ExistsAsync(long userId, long agencyId, long departmentId);
    Task AddAsync(AdminAgencyPermission permission);
    Task DeleteAsync(long userId, long agencyId, long departmentId);
    Task DeleteAllByUserIdAsync(long userId);
}