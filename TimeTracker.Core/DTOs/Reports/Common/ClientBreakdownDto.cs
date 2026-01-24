using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Reports.Common;

public class ClientBreakdownDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int EntriesCount { get; set; }
    public double Percentage { get; set; }
    
    public List<string> Projects { get; set; } = new();
    public List<string> Users { get; set; } = new();
}