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
}