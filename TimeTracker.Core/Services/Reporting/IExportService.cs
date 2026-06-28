using TimeTracker.Core.DTOs.Reports;

namespace TimeTracker.Core.Services.Reporting;

public interface IExportService
{
    Task<byte[]> ExportAllEntriesFlatToExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        ExportColumnsDto columns,
        IEnumerable<long>? agencyIds = null,
        IEnumerable<long>? departmentIds = null,
        IEnumerable<long>? userIds = null);

    Task<byte[]> ExportUserEntriesFlatToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        ExportColumnsDto columns);

    Task<byte[]> ExportUserLoadReportToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task<byte[]> ExportTeamLoadReportToExcelAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task<byte[]> ExportClientReportToExcelAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task<byte[]> ExportTimeSummaryReportToExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        long? agencyId = null,
        long? clientId = null);

    Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        string? title = null,
        bool applyFormatting = true) where T : class;
}