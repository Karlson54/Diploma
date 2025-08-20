// TimeTracker.Data/Repositories/Dictionaries/DictionaryRepository.cs
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Dictionaries;

public class DictionaryRepository<T> : Repository<T>, IDictionaryRepository<T> 
    where T : DictionaryEntity
{
    public DictionaryRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    public async Task<T?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
    }

    public override async Task<IEnumerable<T>> GetActiveAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<bool> IsNameExistsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _dbSet.AnyAsync(e => e.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> IsNameExistsAsync(string name, long excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _dbSet
            .AnyAsync(e => e.Name.ToLower() == name.ToLower() && e.Id != excludeId);
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _dbSet.CountAsync(e => e.IsActive);
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

    public virtual async Task<bool> CanBeDeactivatedAsync(long id)
    {
        // Базовая реализация - всегда можно деактивировать
        // Переопределяется в специализированных репозиториях
        await Task.CompletedTask;
        return true;
    }
}