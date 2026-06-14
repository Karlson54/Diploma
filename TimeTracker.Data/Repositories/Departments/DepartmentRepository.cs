using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Departments;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    public async Task<Department?> GetByNameAndAgencyAsync(string name, long agencyId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(d =>
                d.Name.ToLower() == name.ToLower() &&
                d.AgencyId == agencyId);
    }

    public async Task<IEnumerable<Department>> GetByAgencyAsync(long agencyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.AgencyId == agencyId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetActiveByAgencyAsync(long agencyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(d => d.AgencyId == agencyId && d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<bool> IsNameExistsAsync(string name, long agencyId, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _dbSet
            .Where(d => d.Name.ToLower() == name.ToLower() && d.AgencyId == agencyId);

        if (excludeId.HasValue)
            query = query.Where(d => d.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> HasUsersAsync(long id)
    {
        return await _context.Users
            .AnyAsync(u => u.DepartmentId == id);
    }

    public async Task DeactivateAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null && entity.IsActive)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task ActivateAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null && !entity.IsActive)
        {
            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}