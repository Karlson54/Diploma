// TimeTracker.Data/UnitOfWork/UnitOfWork.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TimeTrackerDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    // Lazy-loaded репозитории (создаются только при первом обращении)
    private IRepository<Agency>? _agencies;
    private IRepository<Market>? _markets;
    private IRepository<ContractingAgency>? _contractingAgencies;
    private IRepository<Client>? _clients;
    private IRepository<Media>? _media;
    private IRepository<JobType>? _jobTypes;
    private IRepository<ProjectBrand>? _projectBrands;
    private IRepository<Role>? _roles;
    private IRepository<User>? _users;
    private IRepository<UserRole>? _userRoles;
    private IRepository<TimeEntry>? _timeEntries;

    public UnitOfWork(TimeTrackerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Репозитории (Lazy Loading pattern)

    public IRepository<Agency> Agencies => 
        _agencies ??= new Repository<Agency>(_context);

    public IRepository<Market> Markets => 
        _markets ??= new Repository<Market>(_context);

    public IRepository<ContractingAgency> ContractingAgencies => 
        _contractingAgencies ??= new Repository<ContractingAgency>(_context);

    public IRepository<Client> Clients => 
        _clients ??= new Repository<Client>(_context);

    public IRepository<Media> Media => 
        _media ??= new Repository<Media>(_context);

    public IRepository<JobType> JobTypes => 
        _jobTypes ??= new Repository<JobType>(_context);

    public IRepository<ProjectBrand> ProjectBrands => 
        _projectBrands ??= new Repository<ProjectBrand>(_context);

    public IRepository<Role> Roles => 
        _roles ??= new Repository<Role>(_context);

    public IRepository<User> Users => 
        _users ??= new Repository<User>(_context);

    public IRepository<UserRole> UserRoles => 
        _userRoles ??= new Repository<UserRole>(_context);

    public IRepository<TimeEntry> TimeEntries => 
        _timeEntries ??= new Repository<TimeEntry>(_context);

    #endregion

    #region Управление транзакциями

    public bool HasActiveTransaction => _currentTransaction != null;

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Транзакция уже активна. Завершите текущую транзакцию перед началом новой.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Нет активной транзакции для коммита.");
        }

        try
        {
            // Сначала сохраняем все изменения в контексте
            await SaveChangesAsync();
            
            // Затем коммитим транзакцию
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            // При ошибке откатываем транзакцию
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            // Очищаем ссылку на транзакцию
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Нет активной транзакции для отката.");
        }

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    #endregion

    #region Сохранение изменений

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            // Обновляем временные метки перед сохранением
            UpdateTimestamps();
            
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Логирование ошибок БД
            throw new InvalidOperationException("Ошибка при сохранении изменений в базу данных.", ex);
        }
    }

    public int SaveChanges()
    {
        try
        {
            UpdateTimestamps();
            return _context.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Ошибка при сохранении изменений в базу данных.", ex);
        }
    }

    /// <summary>
    /// Автоматическое обновление временных меток для измененных сущностей
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = _context.ChangeTracker.Entries<BaseEntity>();
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = currentTime;
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = currentTime;
                    // Предотвращаем изменение CreatedAt
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;
            }
        }
    }

    #endregion

    #region Bulk операции

    public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any()) return;

        var currentTime = DateTime.UtcNow;
        foreach (var entity in entitiesList)
        {
            entity.CreatedAt = currentTime;
        }

        await _context.Set<T>().AddRangeAsync(entitiesList);
    }

    public async Task BulkUpdateAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any()) return;

        var currentTime = DateTime.UtcNow;
        foreach (var entity in entitiesList)
        {
            entity.UpdatedAt = currentTime;
        }

        _context.Set<T>().UpdateRange(entitiesList);
        await Task.CompletedTask; // Для соответствия async паттерну
    }

    public async Task BulkDeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        var entitiesList = entities.ToList();
        if (!entitiesList.Any()) return;

        _context.Set<T>().RemoveRange(entitiesList);
        await Task.CompletedTask; // Для соответствия async паттерну
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Откатываем активную транзакцию при освобождении ресурсов
            if (_currentTransaction != null)
            {
                _currentTransaction.Rollback();
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            _context?.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}