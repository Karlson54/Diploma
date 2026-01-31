using TimeTracker.Core.DTOs.Audit;

namespace TimeTracker.Core.Services.Audit;

public interface IAuditService
{
    // Логування CRUD операцій
    Task LogCreateAsync(
        string entityName,
        long entityId,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogUpdateAsync(
        string entityName,
        long entityId,
        object oldValues,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogDeleteAsync(
        string entityName,
        long entityId,
        object oldValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    // Логування аутентифікації
    Task LogLoginAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent,
        bool success = true,
        string? errorMessage = null);

    Task LogLogoutAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogPasswordChangeAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    // Логування змін прав
    Task LogRoleAssignedAsync(
        long userId,
        string userName,
        long targetUserId,
        string targetUserName,
        long roleId,
        string roleName,
        string ipAddress,
        string userAgent);

    Task LogRoleRemovedAsync(
        long userId,
        string userName,
        long targetUserId,
        string targetUserName,
        long roleId,
        string roleName,
        string ipAddress,
        string userAgent);

    // Bulk операції
    Task LogBulkOperationAsync(
        string action,
        string entityName,
        int affectedCount,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    // Отримання логів
    Task<IEnumerable<AuditLogDto>> GetLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        long? userId = null,
        string? action = null,
        string? entityName = null,
        int pageNumber = 1,
        int pageSize = 50);

    Task<IEnumerable<AuditLogDto>> GetUserActivityAsync(
        long userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    Task<IEnumerable<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        long entityId,
        int pageNumber = 1,
        int pageSize = 50);
}