using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Departments;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByNameAndAgencyAsync(string name, long agencyId);
    Task<IEnumerable<Department>> GetByAgencyAsync(long agencyId);
    Task<IEnumerable<Department>> GetActiveByAgencyAsync(long agencyId);
    Task<bool> IsNameExistsAsync(string name, long agencyId, long? excludeId = null);
    Task<bool> HasUsersAsync(long id);
    Task DeactivateAsync(long id);
    Task ActivateAsync(long id);
}