using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Reports.Common;

namespace TimeTracker.Core.DTOs.Reports;

public class UserLoadReportDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int PeriodDays { get; set; }
    
    // Общая статистика
    public long TotalHoursMs { get; set; }
    public string TotalHours => TimeHelper.FormatHours(TotalHoursMs);
    public string TotalHoursDetailed => TimeHelper.FormatHoursDetailed(TotalHoursMs);
    
    public int TotalEntries { get; set; }
    public int WorkingDaysCount { get; set; }
    public int DaysWithoutEntries { get; set; }
    
    // Средние значения
    public long AverageHoursPerDayMs { get; set; }
    public string AverageHoursPerDay => TimeHelper.FormatHours(AverageHoursPerDayMs);
    
    public long AverageHoursPerWorkingDayMs { get; set; }
    public string AverageHoursPerWorkingDay => TimeHelper.FormatHours(AverageHoursPerWorkingDayMs);
    
    // Детализация
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = new();
    public List<ClientBreakdownDto> ClientBreakdown { get; set; } = new();
    public List<JobTypeBreakdownDto> JobTypeBreakdown { get; set; } = new();
    public List<ProjectBreakdownDto> ProjectBreakdown { get; set; } = new();
}