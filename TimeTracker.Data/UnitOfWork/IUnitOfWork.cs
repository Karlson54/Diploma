// TimeTracker.Data/UnitOfWork/IUnitOfWork.cs

using Microsoft.EntityFrameworkCore.Storage;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    // Репозитории для всех сущностей
    IRepository<Agency> Agencies { get; }
    IRepository<Market> Markets { get; }
    IRepository<ContractingAgency> ContractingAgencies { get; }
    IRepository<Client> Clients { get; }
    IRepository<Media> Media { get; }
    IRepository<JobType> JobTypes { get; }
    IRepository<ProjectBrand> ProjectBrands { get; }
    IRepository<Role> Roles { get; }
    IRepository<User> Users { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<TimeEntry> TimeEntries { get; }
    
    // Управление транзакциями
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    
    // Сохранение изменений
    Task<int> SaveChangesAsync();
    int SaveChanges();
    
    // Проверка состояния
    bool HasActiveTransaction { get; }
    
    // Bulk операции через Unit of Work
    Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : BaseEntity;
    Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : BaseEntity;
    Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity;
}