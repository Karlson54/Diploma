using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Repositories.Common;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetActiveAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
    
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task SaveChangesAsync();
}