namespace TimeTracker.Data.Entities;

public class AuditLog : BaseEntity
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
    public string EntityName { get; set; } = string.Empty; // User, Role, TimeEntry, etc.
    public long? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}