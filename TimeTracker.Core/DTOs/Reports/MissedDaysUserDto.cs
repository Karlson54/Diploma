namespace TimeTracker.Core.DTOs.Reports;

public class MissedDaysUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<DateTime> MissedDates { get; set; } = new();
}