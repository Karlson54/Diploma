using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Audit;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Audit;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task LogCreateAsync(
        string entityName,
        long entityId,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Create,
                EntityName = entityName,
                EntityId = entityId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} (ID: {EntityId}) by User {UserId}",
                AuditAction.Create, entityName, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Create {EntityName} (ID: {EntityId})",
                entityName, entityId);
        }
    }

    public async Task LogRegistrationAsync(
        string userName,
        string email,
        string ipAddress,
        string userAgent,
        bool success = true,
        string? errorMessage = null,
        long? userId = null)
    {
        try
        {
            var newValues = new
            {
                UserName = userName,
                Email = email
            };

            var auditLog = new AuditLog
            {
                UserId = userId ?? 0, // 0 для неудачних спроб
                UserName = userName,
                Action = success ? AuditAction.Register : AuditAction.RegisterFailed,
                EntityName = "Authentication",
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = success,
                ErrorMessage = errorMessage
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} - User: {UserName}, Email: {Email}, Success: {Success}",
                success ? AuditAction.Register : AuditAction.RegisterFailed,
                userName,
                email,
                success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Registration attempt. User: {UserName}, Email: {Email}",
                userName,
                email);
        }
    }

    public async Task LogUpdateAsync(
        string entityName,
        long entityId,
        object oldValues,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Update,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = SerializeObject(oldValues),
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} (ID: {EntityId}) by User {UserId}",
                AuditAction.Update, entityName, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Update {EntityName} (ID: {EntityId})",
                entityName, entityId);
        }
    }

    public async Task LogDeleteAsync(
        string entityName,
        long entityId,
        object oldValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Delete,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = SerializeObject(oldValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} (ID: {EntityId}) by User {UserId}",
                AuditAction.Delete, entityName, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Delete {EntityName} (ID: {EntityId})",
                entityName, entityId);
        }
    }

    public async Task LogLoginAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent,
        bool success = true,
        string? errorMessage = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = success ? AuditAction.Login : AuditAction.LoginFailed,
                EntityName = "Authentication",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = success,
                ErrorMessage = errorMessage
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} by User {UserId} - Success: {Success}",
                success ? AuditAction.Login : AuditAction.LoginFailed,
                userId,
                success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Login attempt");
        }
    }

    public async Task LogLogoutAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Logout,
                EntityName = "Authentication",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Audit: {Action} by User {UserId}", AuditAction.Logout, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Logout");
        }
    }

    public async Task LogPasswordChangeAsync(
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.PasswordChanged,
                EntityName = "User",
                EntityId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} by User {UserId}",
                AuditAction.PasswordChanged, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Password Change");
        }
    }

    public async Task LogRoleAssignedAsync(
        long userId,
        string userName,
        long targetUserId,
        string targetUserName,
        long roleId,
        string roleName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                TargetUserId = targetUserId,
                TargetUserName = targetUserName,
                RoleId = roleId,
                RoleName = roleName
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.RoleAssigned,
                EntityName = "UserRole",
                EntityId = targetUserId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} - Role {RoleName} assigned to User {TargetUserId} by User {UserId}",
                AuditAction.RoleAssigned, roleName, targetUserId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Role Assignment");
        }
    }

    public async Task LogRoleRemovedAsync(
        long userId,
        string userName,
        long targetUserId,
        string targetUserName,
        long roleId,
        string roleName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var oldValues = new
            {
                TargetUserId = targetUserId,
                TargetUserName = targetUserName,
                RoleId = roleId,
                RoleName = roleName
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.RoleRemoved,
                EntityName = "UserRole",
                EntityId = targetUserId,
                OldValues = SerializeObject(oldValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} - Role {RoleName} removed from User {TargetUserId} by User {UserId}",
                AuditAction.RoleRemoved, roleName, targetUserId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Role Removal");
        }
    }

    public async Task LogBulkOperationAsync(
        string action,
        string entityName,
        int affectedCount,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                AffectedCount = affectedCount,
                BulkAction = action
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} - {AffectedCount} records by User {UserId}",
                action, entityName, affectedCount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for Bulk Operation");
        }
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        long? userId = null,
        string? action = null,
        string? entityName = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var logs = await _auditLogRepository.GetLogsAsync(
            fromDate,
            toDate,
            userId,
            action,
            entityName,
            null,
            null,
            pageNumber,
            pageSize);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<IEnumerable<AuditLogDto>> GetUserActivityAsync(
        long userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var logs = await _auditLogRepository.GetUserActivityAsync(
            userId,
            fromDate,
            toDate,
            pageNumber,
            pageSize);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    public async Task<IEnumerable<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        long entityId,
        int pageNumber = 1,
        int pageSize = 50)
    {
        var logs = await _auditLogRepository.GetEntityHistoryAsync(
            entityName,
            entityId,
            pageNumber,
            pageSize);

        return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
    }

    private string? SerializeObject(object? obj)
    {
        if (obj == null) return null;

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize object for audit log");
            return obj.ToString();
        }
    }

    public async Task LogTimeEntryCreatedAsync(
        long timeEntryId,
        long userId,
        string userName,
        DateTime entryDate,
        long hoursMilliseconds,
        object entryDetails,
        long createdByUserId,
        string createdByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                TimeEntryId = timeEntryId,
                UserId = userId,
                UserName = userName,
                EntryDate = entryDate,
                HoursMilliseconds = hoursMilliseconds,
                Details = entryDetails,
                CreatedBy = createdByUserName
            };

            var auditLog = new AuditLog
            {
                UserId = createdByUserId,
                UserName = createdByUserName,
                Action = AuditAction.Create,
                EntityName = "TimeEntry",
                EntityId = timeEntryId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: TimeEntry Created - ID: {TimeEntryId}, User: {UserId} ({UserName}), Date: {Date}, Hours: {Hours}ms by {CreatedBy}",
                timeEntryId, userId, userName, entryDate, hoursMilliseconds, createdByUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for TimeEntry Creation - TimeEntryId: {TimeEntryId}, UserId: {UserId}",
                timeEntryId, userId);
        }
    }

    public async Task LogTimeEntryUpdatedAsync(
        long timeEntryId,
        long userId,
        string userName,
        object oldValues,
        object newValues,
        long updatedByUserId,
        string updatedByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = updatedByUserId,
                UserName = updatedByUserName,
                Action = AuditAction.Update,
                EntityName = "TimeEntry",
                EntityId = timeEntryId,
                OldValues = SerializeObject(oldValues),
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: TimeEntry Updated - ID: {TimeEntryId}, User: {UserId} ({UserName}) by {UpdatedBy}",
                timeEntryId, userId, userName, updatedByUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for TimeEntry Update - TimeEntryId: {TimeEntryId}",
                timeEntryId);
        }
    }

    public async Task LogTimeEntryDeletedAsync(
        long timeEntryId,
        long userId,
        string userName,
        object oldValues,
        long deletedByUserId,
        string deletedByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = deletedByUserId,
                UserName = deletedByUserName,
                Action = AuditAction.Delete,
                EntityName = "TimeEntry",
                EntityId = timeEntryId,
                OldValues = SerializeObject(oldValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: TimeEntry Deleted - ID: {TimeEntryId}, User: {UserId} ({UserName}) by {DeletedBy}",
                timeEntryId, userId, userName, deletedByUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for TimeEntry Deletion - TimeEntryId: {TimeEntryId}",
                timeEntryId);
        }
    }

    public async Task LogTimeEntriesCopiedAsync(
        long userId,
        string userName,
        DateTime sourceDate,
        DateTime targetDate,
        int copiedCount,
        string copyType,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                UserId = userId,
                UserName = userName,
                SourceDate = sourceDate,
                TargetDate = targetDate,
                CopiedCount = copiedCount,
                CopyType = copyType,
                RequestedBy = requestingUserName
            };

            var auditLog = new AuditLog
            {
                UserId = requestingUserId,
                UserName = requestingUserName,
                Action = $"TimeEntryCopy{copyType}", // "TimeEntryCopyDay" или "TimeEntryCopyWeek"
                EntityName = "TimeEntry",
                EntityId = userId, // UserId как EntityId, т.к. копируем для конкретного пользователя
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: TimeEntries Copied ({CopyType}) - User: {UserId} ({UserName}), From: {SourceDate}, To: {TargetDate}, Count: {Count} by {RequestedBy}",
                copyType, userId, userName, sourceDate, targetDate, copiedCount, requestingUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for TimeEntries Copy - UserId: {UserId}, CopyType: {CopyType}",
                userId, copyType);
        }
    }

    public async Task LogTimeEntriesBulkOperationAsync(
        string operation,
        long userId,
        string userName,
        int affectedCount,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                Operation = operation,
                UserId = userId,
                UserName = userName,
                AffectedCount = affectedCount,
                RequestedBy = requestingUserName
            };

            var auditLog = new AuditLog
            {
                UserId = requestingUserId,
                UserName = requestingUserName,
                Action = operation, // "BulkCreate", "BulkUpdate", "BulkDelete"
                EntityName = "TimeEntry",
                EntityId = userId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: TimeEntries {Operation} - User: {UserId} ({UserName}), Count: {AffectedCount} by {RequestedBy}",
                operation, userId, userName, affectedCount, requestingUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for TimeEntries Bulk Operation - Operation: {Operation}, UserId: {UserId}",
                operation, userId);
        }
    }

    public async Task LogDictionaryCreatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Create,
                EntityName = dictionaryType,
                EntityId = dictionaryId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} '{Name}' (ID: {EntityId}) by User {UserId}",
                AuditAction.Create, dictionaryType, dictionaryName, dictionaryId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Create {EntityName} '{Name}' (ID: {EntityId})",
                dictionaryType, dictionaryName, dictionaryId);
        }
    }

    public async Task LogDictionaryUpdatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        object oldValues,
        object newValues,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Update,
                EntityName = dictionaryType,
                EntityId = dictionaryId,
                OldValues = SerializeObject(oldValues),
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} '{Name}' (ID: {EntityId}) by User {UserId}",
                AuditAction.Update, dictionaryType, dictionaryName, dictionaryId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Update {EntityName} '{Name}' (ID: {EntityId})",
                dictionaryType, dictionaryName, dictionaryId);
        }
    }

    public async Task LogDictionaryActivatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.DictionaryActivated,
                EntityName = dictionaryType,
                EntityId = dictionaryId,
                NewValues = SerializeObject(new { Name = dictionaryName, IsActive = true }),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} '{Name}' (ID: {EntityId}) by User {UserId}",
                AuditAction.DictionaryActivated, dictionaryType, dictionaryName, dictionaryId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Activate {EntityName} '{Name}' (ID: {EntityId})",
                dictionaryType, dictionaryName, dictionaryId);
        }
    }

    public async Task LogDictionaryDeactivatedAsync(
        string dictionaryType,
        long dictionaryId,
        string dictionaryName,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.DictionaryDeactivated,
                EntityName = dictionaryType,
                EntityId = dictionaryId,
                OldValues = SerializeObject(new { Name = dictionaryName, IsActive = true }),
                NewValues = SerializeObject(new { Name = dictionaryName, IsActive = false }),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: {Action} {EntityName} '{Name}' (ID: {EntityId}) by User {UserId}",
                AuditAction.DictionaryDeactivated, dictionaryType, dictionaryName, dictionaryId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for Deactivate {EntityName} '{Name}' (ID: {EntityId})",
                dictionaryType, dictionaryName, dictionaryId);
        }
    }

    public async Task LogUserCreatedAsync(
        long userId,
        string userName,
        string email,
        long agencyId,
        long createdByUserId,
        string createdByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                UserId = userId,
                UserName = userName,
                Email = email,
                AgencyId = agencyId
            };

            var auditLog = new AuditLog
            {
                UserId = createdByUserId,
                UserName = createdByUserName,
                Action = AuditAction.Create,
                EntityName = "User",
                EntityId = userId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: User Created - ID: {UserId}, Name: '{UserName}', Email: '{Email}' by User {CreatedBy}",
                userId, userName, email, createdByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for User Creation - UserId: {UserId}, UserName: '{UserName}'",
                userId, userName);
        }
    }

    public async Task LogUserUpdatedAsync(
        long userId,
        string userName,
        object oldValues,
        object newValues,
        long updatedByUserId,
        string updatedByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = updatedByUserId,
                UserName = updatedByUserName,
                Action = AuditAction.Update,
                EntityName = "User",
                EntityId = userId,
                OldValues = SerializeObject(oldValues),
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: User Updated - ID: {UserId}, Name: '{UserName}' by User {UpdatedBy}",
                userId, userName, updatedByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for User Update - UserId: {UserId}",
                userId);
        }
    }

    public async Task LogUserActivatedAsync(
        long userId,
        string userName,
        long activatedByUserId,
        string activatedByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                UserId = userId,
                UserName = userName,
                IsActive = true
            };

            var auditLog = new AuditLog
            {
                UserId = activatedByUserId,
                UserName = activatedByUserName,
                Action = AuditAction.UserActivated,
                EntityName = "User",
                EntityId = userId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: User Activated - ID: {UserId}, Name: '{UserName}' by User {ActivatedBy}",
                userId, userName, activatedByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for User Activation - UserId: {UserId}",
                userId);
        }
    }

    public async Task LogUserDeactivatedAsync(
        long userId,
        string userName,
        long deactivatedByUserId,
        string deactivatedByUserName,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var oldValues = new
            {
                UserId = userId,
                UserName = userName,
                IsActive = true
            };

            var newValues = new
            {
                UserId = userId,
                UserName = userName,
                IsActive = false
            };

            var auditLog = new AuditLog
            {
                UserId = deactivatedByUserId,
                UserName = deactivatedByUserName,
                Action = AuditAction.UserDeactivated,
                EntityName = "User",
                EntityId = userId,
                OldValues = SerializeObject(oldValues),
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: User Deactivated - ID: {UserId}, Name: '{UserName}' by User {DeactivatedBy}",
                userId, userName, deactivatedByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for User Deactivation - UserId: {UserId}",
                userId);
        }
    }

    public async Task LogUserPasswordChangedAsync(
        long userId,
        string userName,
        long changedByUserId,
        string changedByUserName,
        bool isSelfChange,
        string ipAddress,
        string userAgent)
    {
        try
        {
            var newValues = new
            {
                UserId = userId,
                UserName = userName,
                IsSelfChange = isSelfChange,
                ChangedBy = changedByUserId
            };

            var auditLog = new AuditLog
            {
                UserId = changedByUserId,
                UserName = changedByUserName,
                Action = AuditAction.PasswordChanged,
                EntityName = "User",
                EntityId = userId,
                NewValues = SerializeObject(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = true
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Audit: User Password Changed - ID: {UserId}, Name: '{UserName}', IsSelfChange: {IsSelfChange}, ChangedBy: {ChangedBy}",
                userId, userName, isSelfChange, changedByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to log audit for User Password Change - UserId: {UserId}",
                userId);
        }
    }
}