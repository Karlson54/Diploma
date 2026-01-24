using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Reports.Common;

namespace TimeTracker.Core.DTOs.Reports;

public class TeamLoadReportDto
{
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Общая статистика команды
    public long TotalTeamHoursMs { get; set; }
    public string TotalTeamHours => TimeHelper.FormatHours(TotalTeamHoursMs);
    
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    
    // Средние значения
    public long AverageHoursPerMemberMs { get; set; }
    public string AverageHoursPerMember => TimeHelper.FormatHours(AverageHoursPerMemberMs);
    
    // Детализация по сотрудникам
    public List<UserLoadSummaryDto> MembersLoad { get; set; } = new();
    
    // Топ клиенты команды
    public List<ClientBreakdownDto> TopClients { get; set; } = new();
    
    // Распределение по типам работ
    public List<JobTypeBreakdownDto> JobTypeDistribution { get; set; } = new();
}