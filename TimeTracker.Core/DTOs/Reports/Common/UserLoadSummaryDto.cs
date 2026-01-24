using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class UserLoadSummaryDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int EntriesCount { get; set; }
    public int WorkingDays { get; set; }
    
    public long AverageHoursPerDayMs { get; set; }
    public string AverageHoursPerDay => TimeHelper.FormatHours(AverageHoursPerDayMs);
    
    public double LoadPercentage { get; set; } // % от общего времени команды
}