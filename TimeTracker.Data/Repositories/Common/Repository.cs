// TimeTracker.Data/Repositories/Common/Repository.cs
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Repositories.Common;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly TimeTrackerDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(TimeTrackerDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Базовые операции (без загрузки связанных данных)

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetActiveAsync()
    {
        if (typeof(DictionaryEntity).IsAssignableFrom(typeof(T)))
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => ((DictionaryEntity)(object)e).IsActive)
                .ToListAsync();
        }
        
        return await GetAllAsync();
    }

    #endregion

    #region IQueryable методы (максимальная гибкость)

    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public virtual IQueryable<T> GetActiveQueryable()
    {
        if (typeof(DictionaryEntity).IsAssignableFrom(typeof(T)))
        {
            return _dbSet.Where(e => ((DictionaryEntity)(object)e).IsActive);
        }
        
        return _dbSet.AsQueryable();
    }

    #endregion

    #region Explicit Loading (правильный подход)

    public virtual async Task LoadCollectionAsync<TProperty>(
        T entity, 
        Expression<Func<T, IEnumerable<TProperty>>> navigationProperty)
        where TProperty : class
    {
        await _context.Entry(entity)
            .Collection(navigationProperty)
            .LoadAsync();
    }

    public virtual async Task LoadReferenceAsync<TProperty>(
        T entity, 
        Expression<Func<T, TProperty?>> navigationProperty)
        where TProperty : class
    {
        await _context.Entry(entity)
            .Reference(navigationProperty)
            .LoadAsync();
    }

    public virtual async Task<int> LoadCollectionCountAsync<TProperty>(
        T entity, 
        Expression<Func<T, IEnumerable<TProperty>>> navigationProperty)
        where TProperty : class
    {
        return await _context.Entry(entity)
            .Collection(navigationProperty)
            .Query()
            .CountAsync();
    }

    #endregion

    #region Проекции для оптимизации

    public virtual async Task<TResult?> GetProjectionAsync<TResult>(
        long id, 
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<TResult>> GetProjectionsAsync<TResult>(
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet
            .AsNoTracking()
            .Select(selector)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<TResult>> GetProjectionsAsync<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(filter)
            .Select(selector)
            .ToListAsync();
    }

    #endregion

    #region Поиск и фильтрация

    public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null 
            ? await _dbSet.CountAsync() 
            : await _dbSet.CountAsync(predicate);
    }

    #endregion

    #region Пагинация

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderByDescending 
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);
        }
        else
        {
            // Дефолтная сортировка по Id для стабильной пагинации
            query = query.OrderBy(e => e.Id);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region CRUD операции

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        var result = await _dbSet.AddAsync(entity);
        return result.Entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var entitiesList = entities.ToList();
        var currentTime = DateTime.UtcNow;
        
        foreach (var entity in entitiesList)
        {
            entity.CreatedAt = currentTime;
        }
        
        await _dbSet.AddRangeAsync(entitiesList);
        return entitiesList;
    }

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        var currentTime = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.UpdatedAt = currentTime;
        }
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task DeleteAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    #endregion

    #region Проверки существования

    public virtual async Task<bool> ExistsAsync(long id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    #endregion
}