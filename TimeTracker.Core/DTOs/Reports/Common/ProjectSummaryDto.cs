using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class ProjectSummaryDto
{
    public long ProjectBrandId { get; set; }
    public string ProjectBrandName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int UsersCount { get; set; }
    public double Percentage { get; set; }
}