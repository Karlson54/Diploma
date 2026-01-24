using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class UserContributionDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AgencyName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int EntriesCount { get; set; }
    public double ContributionPercentage { get; set; }
}