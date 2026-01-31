using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Audit;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetLogsAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? userId,
        string? action,
        string? entityName,
        long? entityId,
        bool? success,
        int pageNumber,
        int pageSize);

    Task<int> GetLogsCountAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? userId,
        string? action,
        string? entityName,
        long? entityId,
        bool? success);

    Task<IEnumerable<AuditLog>> GetUserActivityAsync(
        string userId,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);

    Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(
        string entityName,
        long entityId,
        int pageNumber,
        int pageSize);

    Task<Dictionary<string, int>> GetActionStatisticsAsync(
        string userId,
        DateTime fromDate,
        DateTime toDate);

    Task<int> DeleteOldLogsAsync(DateTime beforeDate);
}