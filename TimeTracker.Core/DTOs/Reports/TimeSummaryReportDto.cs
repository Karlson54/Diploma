using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Reports.Common;

namespace TimeTracker.Core.DTOs.Reports;

public class TimeSummaryReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Фильтры (если применялись)
    public long? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public long? ClientId { get; set; }
    public string? ClientName { get; set; }
    
    // Общая статистика
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    
    public int TotalEntries { get; set; }
    public int TotalUsers { get; set; }
    public int TotalClients { get; set; }
    public int TotalProjects { get; set; }
    
    // Топы
    public List<UserLoadSummaryDto> TopUsers { get; set; } = new();
    public List<ClientSummaryDto> TopClients { get; set; } = new();
    public List<ProjectSummaryDto> TopProjects { get; set; } = new();
    
    // Распределения
    public List<AgencyBreakdownDto> AgencyBreakdown { get; set; } = new();
    public List<JobTypeBreakdownDto> JobTypeBreakdown { get; set; } = new();
    public List<MediaBreakdownDto> MediaBreakdown { get; set; } = new();
    
    // Временная динамика
    public List<WeeklyBreakdownDto> WeeklyTrends { get; set; } = new();
}