namespace TimeTracker.Core.Services.Reporting;

public interface IExportService
{
    // Export to Excel (з локалізацією)
    Task<byte[]> ExportUserLoadReportToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportTeamLoadReportToExcelAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportClientReportToExcelAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportTimeSummaryReportToExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        long? agencyId = null,
        long? clientId = null,
        string locale = "uk");

    // Export to CSV (з локалізацією)
    Task<byte[]> ExportUserLoadReportToCsvAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportTeamLoadReportToCsvAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportClientReportToCsvAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk");

    Task<byte[]> ExportTimeSummaryReportToCsvAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        long? agencyId = null,
        long? clientId = null,
        string locale = "uk");

    // Generic export (з опціями стилізації)
    Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        string? title = null,
        string locale = "uk",
        bool applyFormatting = true) where T : class;

    Task<byte[]> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string locale = "uk") where T : class;
}