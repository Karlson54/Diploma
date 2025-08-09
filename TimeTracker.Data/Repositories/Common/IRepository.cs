// TimeTracker.Data/Repositories/Common/IRepository.cs
using System.Linq.Expressions;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Repositories.Common;

public interface IRepository<T> where T : BaseEntity
{
    // Базовые операции без загрузки связанных данных
    Task<T?> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetActiveAsync();
    
    // Методы для работы с IQueryable (максимальная гибкость)
    IQueryable<T> GetQueryable();
    IQueryable<T> GetActiveQueryable();
    
    // Explicit Loading методы
    Task LoadCollectionAsync<TProperty>(T entity, Expression<Func<T, IEnumerable<TProperty>>> navigationProperty)
        where TProperty : class;
    
    Task LoadReferenceAsync<TProperty>(T entity, Expression<Func<T, TProperty?>> navigationProperty)
        where TProperty : class;
    
    Task<int> LoadCollectionCountAsync<TProperty>(T entity, Expression<Func<T, IEnumerable<TProperty>>> navigationProperty)
        where TProperty : class;
    
    // Проекции для оптимизации (загружаем только нужные поля)
    Task<TResult?> GetProjectionAsync<TResult>(long id, Expression<Func<T, TResult>> selector);
    Task<IEnumerable<TResult>> GetProjectionsAsync<TResult>(Expression<Func<T, TResult>> selector);
    Task<IEnumerable<TResult>> GetProjectionsAsync<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector);
    
    // Фильтрация и поиск
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // Пагинация
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false);
    
    // CRUD операции
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    Task DeleteAsync(long id);
    void DeleteRange(IEnumerable<T> entities);
    
    // Проверки существования
    Task<bool> ExistsAsync(long id);
}