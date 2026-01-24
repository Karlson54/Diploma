using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Reports.Common;

namespace TimeTracker.Core.DTOs.Reports;

public class ProjectReportDto
{
    public long ProjectBrandId { get; set; }
    public string ProjectBrandName { get; set; } = string.Empty;
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Общая статистика
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int TotalEntries { get; set; }
    public int UniqueUsers { get; set; }
    
    // Связанные сущности
    public List<string> Clients { get; set; } = new();
    public List<string> Agencies { get; set; } = new();
    
    // Детализация
    public List<UserContributionDto> UserContributions { get; set; } = new();
    public List<JobTypeBreakdownDto> JobTypeBreakdown { get; set; } = new();
    public List<MediaBreakdownDto> MediaBreakdown { get; set; } = new();
    public List<WeeklyBreakdownDto> WeeklyBreakdown { get; set; } = new();
}