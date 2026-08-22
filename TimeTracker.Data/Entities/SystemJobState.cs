namespace TimeTracker.Data.Entities;

public class SystemJobState : BaseEntity
{
    public string JobName { get; set; } = string.Empty;
    public DateTime? LastRunAt { get; set; }
    public string LastStatus { get; set; } = string.Empty; // "Success" / "Failed" / "Running"
    public string? LastErrorMessage { get; set; }
}