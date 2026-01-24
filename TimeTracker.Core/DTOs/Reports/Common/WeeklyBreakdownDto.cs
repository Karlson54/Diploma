using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class WeeklyBreakdownDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int WeekNumber { get; set; }
    
    public long HoursMs { get; set; }
    public string Hours => TimeHelper.FormatHours(HoursMs);
    
    public int EntriesCount { get; set; }
    public int WorkingDays { get; set; }
    
    public long AverageHoursPerDayMs { get; set; }
    public string AverageHoursPerDay => TimeHelper.FormatHours(AverageHoursPerDayMs);
}