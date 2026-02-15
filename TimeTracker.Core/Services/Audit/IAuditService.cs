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
    Task LogRegistrationAsync(
        string userName,
        string email,
        string ipAddress,
        string userAgent,
        bool success = true,
        string? errorMessage = null,
        long? userId = null);

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

    // Логування операцій зі словниками (Dictionaries)
    Task LogDictionaryCreatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogDictionaryUpdatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        object oldValues,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogDictionaryActivatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    Task LogDictionaryDeactivatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        long userId,
        string userName,
        string ipAddress,
        string userAgent);

    // User Management
    Task LogUserCreatedAsync(
        long userId,
        string userName,
        string email,
        long agencyId,
        long createdByUserId,
        string createdByUserName,
        string ipAddress,
        string userAgent);

    Task LogUserUpdatedAsync(
        long userId,
        string userName,
        object oldValues,
        object newValues,
        long updatedByUserId,
        string updatedByUserName,
        string ipAddress,
        string userAgent);

    Task LogUserActivatedAsync(
        long userId,
        string userName,
        long activatedByUserId,
        string activatedByUserName,
        string ipAddress,
        string userAgent);

    Task LogUserDeactivatedAsync(
        long userId,
        string userName,
        long deactivatedByUserId,
        string deactivatedByUserName,
        string ipAddress,
        string userAgent);

    Task LogUserPasswordChangedAsync(
        long userId,
        string userName,
        long changedByUserId,
        string changedByUserName,
        bool isSelfChange,
        string ipAddress,
        string userAgent);
    
    // TimeEntry Management
    Task LogTimeEntryCreatedAsync(
        long timeEntryId,
        long userId,
        string userName,
        DateTime entryDate,
        long hoursMilliseconds,
        object entryDetails,
        long createdByUserId,
        string createdByUserName,
        string ipAddress,
        string userAgent);

    Task LogTimeEntryUpdatedAsync(
        long timeEntryId,
        long userId,
        string userName,
        object oldValues,
        object newValues,
        long updatedByUserId,
        string updatedByUserName,
        string ipAddress,
        string userAgent);

    Task LogTimeEntryDeletedAsync(
        long timeEntryId,
        long userId,
        string userName,
        object oldValues,
        long deletedByUserId,
        string deletedByUserName,
        string ipAddress,
        string userAgent);

    Task LogTimeEntriesCopiedAsync(
        long userId,
        string userName,
        DateTime sourceDate,
        DateTime targetDate,
        int copiedCount,
        string copyType, // "Day" или "Week"
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task LogTimeEntriesBulkOperationAsync(
        string operation, // "BulkCreate", "BulkUpdate", "BulkDelete"
        long userId,
        string userName,
        int affectedCount,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);
}