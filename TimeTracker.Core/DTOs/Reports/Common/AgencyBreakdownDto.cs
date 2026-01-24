using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class AgencyBreakdownDto
{
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int TotalUsers { get; set; }
    public int EntriesCount { get; set; }
    public double Percentage { get; set; }
}