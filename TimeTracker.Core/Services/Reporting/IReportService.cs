using TimeTracker.Core.DTOs.Reports;
using TimeTracker.Core.DTOs.Reports.Common;
using TimeTracker.Core.DTOs.TimeEntries;

namespace TimeTracker.Core.Services.Reporting;

public interface IReportService
{
    Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesForExportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<IEnumerable<TimeEntryDto>> GetUserTimeEntriesForExportAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<UserLoadReportDto> GetUserLoadReportAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<TeamLoadReportDto> GetTeamLoadReportAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<ClientReportDto> GetClientReportAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<IEnumerable<ClientSummaryDto>> GetTopClientsReportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        long? agencyId = null,
        int top = 10);

    Task<ProjectReportDto> GetProjectReportAsync(
        string projectBrandName,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId);

    Task<TimeSummaryReportDto> GetTimeSummaryReportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        long? agencyId = null,
        long? clientId = null);

    Task<bool> CanUserAccessReportAsync(long requestingUserId, long? targetUserId = null);
}