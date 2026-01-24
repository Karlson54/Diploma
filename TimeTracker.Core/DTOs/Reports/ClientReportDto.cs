using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Reports.Common;

namespace TimeTracker.Core.DTOs.Reports;

public class ClientReportDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Общая статистика
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int TotalEntries { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueProjects { get; set; }
    
    // Детализация
    public List<UserContributionDto> UserContributions { get; set; } = new();
    public List<ProjectBreakdownDto> ProjectBreakdown { get; set; } = new();
    public List<JobTypeBreakdownDto> JobTypeBreakdown { get; set; } = new();
    public List<MediaBreakdownDto> MediaBreakdown { get; set; } = new();
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = new();
}