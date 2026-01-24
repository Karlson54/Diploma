using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class DailyBreakdownDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek => Date.ToString("dddd");
    
    public long HoursMs { get; set; }
    public string Hours => TimeHelper.FormatHours(HoursMs);
    
    public int EntriesCount { get; set; }
    public List<string> Clients { get; set; } = new();
    public List<string> Projects { get; set; } = new();
}