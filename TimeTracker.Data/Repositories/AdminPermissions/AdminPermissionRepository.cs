using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Repositories.AdminPermissions;

public class AdminPermissionRepository : IAdminPermissionRepository
{
    private readonly TimeTrackerDbContext _context;

    public AdminPermissionRepository(TimeTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AdminAgencyPermission>> GetByUserIdAsync(long userId)
    {
        return await _context.AdminAgencyPermissions
            .AsNoTracking()
            .Include(p => p.Agency)
            .Include(p => p.Department)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Agency.Name)
            .ThenBy(p => p.Department.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(long userId, long agencyId, long departmentId)
    {
        return await _context.AdminAgencyPermissions
            .AnyAsync(p => p.UserId == userId
                        && p.AgencyId == agencyId
                        && p.DepartmentId == departmentId);
    }

    public async Task AddAsync(AdminAgencyPermission permission)
    {
        await _context.AdminAgencyPermissions.AddAsync(permission);
    }

    public async Task DeleteAsync(long userId, long agencyId, long departmentId)
    {
        var permission = await _context.AdminAgencyPermissions
            .FirstOrDefaultAsync(p => p.UserId == userId
                                   && p.AgencyId == agencyId
                                   && p.DepartmentId == departmentId);

        if (permission != null)
            _context.AdminAgencyPermissions.Remove(permission);
    }

    public async Task DeleteAllByUserIdAsync(long userId)
    {
        var permissions = await _context.AdminAgencyPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();

        _context.AdminAgencyPermissions.RemoveRange(permissions);
    }
}