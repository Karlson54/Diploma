namespace TimeTracker.Core.DTOs.Audit;

public class UserActivitySummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalActions { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public List<AuditLogDto> RecentActions { get; set; } = new();
}