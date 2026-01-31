using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Audit;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        long? userId,
        string? action,
        string? entityName,
        long? entityId,
        bool? success,
        int pageNumber,
        int pageSize)
    {
        var query = BuildLogsQuery(fromDate, toDate, userId, action, entityName, entityId, success);

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetLogsCountAsync(
        DateTime? fromDate,
        DateTime? toDate,
        long? userId,
        string? action,
        string? entityName,
        long? entityId,
        bool? success)
    {
        var query = BuildLogsQuery(fromDate, toDate, userId, action, entityName, entityId, success);
        return await query.CountAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetUserActivityAsync(
        long userId,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(log => log.UserId == userId);

        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(
        string entityName,
        long entityId,
        int pageNumber,
        int pageSize)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(log => log.EntityName == entityName && log.EntityId == entityId)
            .OrderByDescending(log => log.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetActionStatisticsAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(log => log.UserId == userId &&
                         log.CreatedAt >= fromDate &&
                         log.CreatedAt <= toDate)
            .GroupBy(log => log.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);
    }

    public async Task<int> DeleteOldLogsAsync(DateTime beforeDate)
    {
        var logsToDelete = await _dbSet
            .Where(log => log.CreatedAt < beforeDate)
            .ToListAsync();

        _dbSet.RemoveRange(logsToDelete);
        return logsToDelete.Count;
    }

    private IQueryable<AuditLog> BuildLogsQuery(
        DateTime? fromDate,
        DateTime? toDate,
        long? userId,
        string? action,
        string? entityName,
        long? entityId,
        bool? success)
    {
        var query = _dbSet.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= toDate.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(log => log.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(log => log.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(log => log.EntityName == entityName);
        }

        if (entityId.HasValue)
        {
            query = query.Where(log => log.EntityId == entityId.Value);
        }

        if (success.HasValue)
        {
            query = query.Where(log => log.Success == success.Value);
        }

        return query;
    }
}