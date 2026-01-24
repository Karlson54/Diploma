using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class MediaBreakdownDto
{
    public long MediaId { get; set; }
    public string MediaName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int EntriesCount { get; set; }
    public double Percentage { get; set; }
}