using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class ClientSummaryDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHoursDecimal(TotalHoursMs);
    
    public int ProjectsCount { get; set; }
    public int UsersCount { get; set; }
    public double Percentage { get; set; }
}