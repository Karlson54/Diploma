namespace TimeTracker.Core.DTOs.Reports;

public class InactiveUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime? LastEntryDate { get; set; }
}