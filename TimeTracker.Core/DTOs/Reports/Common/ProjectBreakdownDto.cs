using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class ProjectBreakdownDto
{
    public long ProjectBrandId { get; set; }
    public string ProjectBrandName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int EntriesCount { get; set; }
    public double Percentage { get; set; }
    
    public string? ClientName { get; set; }
}