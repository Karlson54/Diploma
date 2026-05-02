using ClosedXML.Excel;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Reports;
using TimeTracker.Core.DTOs.Reports.Common;
using TimeTracker.Core.Services.Audit;
using static TimeTracker.Core.Common.ExportConstants;
using TimeTracker.Core.DTOs.Reports;

namespace TimeTracker.Core.Services.Reporting;

public class ExportService : IExportService
{
    private readonly IReportService _reportService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IReportService reportService,
        IAuditService auditService,
        ILogger<ExportService> logger)
    {
        _reportService = reportService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<byte[]> ExportUserLoadReportToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetUserLoadReportAsync(
                userId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var sheetName = locale.ToLower() == "uk" ? "Навантаження користувача" : "User Load Report";
            var worksheet = workbook.Worksheets.Add(sheetName);

            var currentRow = 1;

            currentRow = AddReportTitle(
                worksheet,
                currentRow,
                GetLocalizedText("User Load Report", locale),
                locale);

            currentRow = AddUserInfo(worksheet, currentRow, report, locale);
            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
            currentRow++;

            currentRow = AddUserStatistics(worksheet, currentRow, report, locale);
            currentRow++;

            if (report.DailyBreakdown.Any())
            {
                currentRow = AddDailyBreakdown(worksheet, currentRow, report.DailyBreakdown, locale);
                currentRow++;
            }

            if (report.ClientBreakdown.Any())
            {
                currentRow = AddClientBreakdown(worksheet, currentRow, report.ClientBreakdown, locale);
                currentRow++;
            }

            if (report.JobTypeBreakdown.Any())
            {
                currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown, locale);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "UserLoad",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: report.UserName,
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті User Load Report для користувача {UserId}",
                userId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "UserLoad",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportTeamLoadReportToExcelAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetTeamLoadReportAsync(
                agencyId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(GetLocalizedText("Team Load Report", locale));

            var currentRow = 1;

            currentRow = AddReportTitle(
                worksheet,
                currentRow,
                GetLocalizedText("Team Load Report", locale),
                locale);

            worksheet.Cell(currentRow, 1).Value = GetLocalizedText("Agency", locale) + ":";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = report.AgencyName;
            currentRow++;

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
            currentRow++;

            currentRow = AddTeamStatistics(worksheet, currentRow, report, locale);
            currentRow++;

            if (report.MembersLoad.Any())
            {
                currentRow = AddMembersLoad(worksheet, currentRow, report.MembersLoad, locale);
                currentRow++;
            }

            if (report.TopClients.Any())
            {
                currentRow = AddTopClientsFromClientBreakdown(worksheet, currentRow, report.TopClients, locale);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TeamLoad",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: report.AgencyName,
                reportParams: new { AgencyId = agencyId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Team Load Report для агентства {AgencyId}",
                agencyId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TeamLoad",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { AgencyId = agencyId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportClientReportToExcelAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetClientReportAsync(
                clientId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(GetLocalizedText("Client Report", locale));

            var currentRow = 1;

            currentRow = AddReportTitle(
                worksheet,
                currentRow,
                GetLocalizedText("Client Report", locale),
                locale);

            worksheet.Cell(currentRow, 1).Value = GetLocalizedText("Client", locale) + ":";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = report.ClientName;
            currentRow++;

            if (!string.IsNullOrEmpty(report.ClientEmail))
            {
                worksheet.Cell(currentRow, 1).Value = "Email:";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = report.ClientEmail;
                currentRow++;
            }

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
            currentRow++;

            currentRow = AddClientStatistics(worksheet, currentRow, report, locale);
            currentRow++;

            if (report.ProjectBreakdown.Any())
            {
                currentRow = AddProjectBreakdown(worksheet, currentRow, report.ProjectBreakdown, locale);
                currentRow++;
            }

            if (report.JobTypeBreakdown.Any())
            {
                currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown, locale);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "Client",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: report.ClientName,
                reportParams: new { ClientId = clientId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Client Report для клієнта {ClientId}",
                clientId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "Client",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { ClientId = clientId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportTimeSummaryReportToExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        long? agencyId = null,
        long? clientId = null,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetTimeSummaryReportAsync(
                fromDate, toDate, requestingUserId, agencyId, clientId);

            using var workbook = new XLWorkbook();
            var sheetName = locale.ToLower() == "uk" ? "Зведений звіт" : "Time Summary Report";
            var worksheet = workbook.Worksheets.Add(sheetName);

            var currentRow = 1;

            currentRow = AddReportTitle(
                worksheet,
                currentRow,
                GetLocalizedText("Time Summary Report", locale),
                locale);

            if (agencyId.HasValue)
            {
                worksheet.Cell(currentRow, 1).Value = GetLocalizedText("Agency", locale) + ":";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = report.AgencyName ?? "";
                currentRow++;
            }

            if (clientId.HasValue)
            {
                worksheet.Cell(currentRow, 1).Value = GetLocalizedText("Client", locale) + ":";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = report.ClientName ?? "";
                currentRow++;
            }

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
            currentRow++;

            currentRow = AddSummaryStatistics(worksheet, currentRow, report, locale);
            currentRow++;

            if (report.TopUsers.Any())
            {
                currentRow = AddTopUsers(worksheet, currentRow, report.TopUsers, locale);
                currentRow++;
            }

            if (report.TopClients.Any())
            {
                currentRow = AddTopClientsFromClientSummary(worksheet, currentRow, report.TopClients, locale);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TimeSummary",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: report.AgencyName ?? "System",
                reportParams: new { FromDate = fromDate, ToDate = toDate, AgencyId = agencyId, ClientId = clientId },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Time Summary Report");

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TimeSummary",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { FromDate = fromDate, ToDate = toDate, AgencyId = agencyId, ClientId = clientId },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportUserLoadReportToCsvAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetUserLoadReportAsync(
                userId, fromDate, toDate, requestingUserId);

            var records = new List<UserLoadCsvRecord>();

            foreach (var day in report.DailyBreakdown)
            {
                records.Add(new UserLoadCsvRecord
                {
                    UserName = report.UserName,
                    UserEmail = report.UserEmail,
                    AgencyName = report.AgencyName,
                    Date = day.Date.ToString("yyyy-MM-dd"),
                    DayOfWeek = day.DayOfWeek,
                    Hours = day.Hours,
                    EntriesCount = day.EntriesCount
                });
            }

            var result = ExportToCsvInternal(records, locale);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "UserLoad",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: report.UserName,
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті User Load Report CSV для користувача {UserId}",
                userId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "UserLoad",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportTeamLoadReportToCsvAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetTeamLoadReportAsync(
                agencyId, fromDate, toDate, requestingUserId);

            var records = report.MembersLoad.Select(m => new TeamLoadCsvRecord
            {
                AgencyName = report.AgencyName,
                UserName = m.UserName,
                UserEmail = m.UserEmail,
                TotalHours = m.TotalHours,
                EntriesCount = m.EntriesCount,
                WorkingDays = m.WorkingDays,
                AverageHoursPerDay = m.AverageHoursPerDay,
                LoadPercentage = m.LoadPercentage
            }).ToList();

            var result = ExportToCsvInternal(records, locale);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TeamLoad",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: report.AgencyName,
                reportParams: new { AgencyId = agencyId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Team Load Report CSV для агентства {AgencyId}",
                agencyId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TeamLoad",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { AgencyId = agencyId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportClientReportToCsvAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetClientReportAsync(
                clientId, fromDate, toDate, requestingUserId);

            var records = report.ProjectBreakdown.Select(p => new ClientReportCsvRecord
            {
                ClientName = report.ClientName,
                ProjectName = p.ProjectBrandName,
                TotalHours = p.TotalHours,
                EntriesCount = p.EntriesCount,
                Percentage = p.Percentage
            }).ToList();

            var result = ExportToCsvInternal(records, locale);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "Client",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: report.ClientName,
                reportParams: new { ClientId = clientId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Client Report CSV для клієнта {ClientId}",
                clientId);

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "Client",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { ClientId = clientId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public async Task<byte[]> ExportTimeSummaryReportToCsvAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        long? agencyId = null,
        long? clientId = null,
        string locale = "uk")
    {
        try
        {
            var report = await _reportService.GetTimeSummaryReportAsync(
                fromDate, toDate, requestingUserId, agencyId, clientId);

            var records = report.TopClients.Select(c => new TimeSummaryCsvRecord
            {
                ClientName = c.ClientName,
                TotalHours = c.TotalHours,
                ProjectsCount = c.ProjectsCount,
                UsersCount = c.UsersCount,
                Percentage = c.Percentage
            }).ToList();

            var result = ExportToCsvInternal(records, locale);

            // Аудит успішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TimeSummary",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: report.AgencyName ?? "System",
                reportParams: new { FromDate = fromDate, ToDate = toDate, AgencyId = agencyId, ClientId = clientId },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при експорті Time Summary Report CSV");

            // Аудит неуспішного експорту
            await _auditService.LogReportExportedAsync(
                reportType: "TimeSummary",
                exportFormat: "CSV",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { FromDate = fromDate, ToDate = toDate, AgencyId = agencyId, ClientId = clientId },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    public Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        string? title = null,
        string locale = "uk",
        bool applyFormatting = true) where T : class
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var dataList = data.ToList();
        if (!dataList.Any())
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        var currentRow = 1;

        // Додаємо заголовок якщо є
        if (!string.IsNullOrEmpty(title))
        {
            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = ExcelStyles.TitleFontSize;
            currentRow += 2;
        }

        // Додаємо дані через вбудовану функцію ClosedXML
        var table = worksheet.Cell(currentRow, 1).InsertTable(dataList);

        if (applyFormatting)
        {
            // Стилізація заголовків
            var headerRow = table.HeadersRow();
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
            headerRow.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);

            // Альтернативні кольори рядків
            for (int i = 0; i < table.DataRange.RowCount(); i++)
            {
                if (i % 2 == 0)
                {
                    table.DataRange.Row(i + 1).Style.Fill.BackgroundColor =
                        XLColor.FromHtml(ExcelStyles.AlternateRowColor);
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string locale = "uk") where T : class
    {
        return Task.FromResult(ExportToCsvInternal(data, locale));
    }

    private int AddReportTitle(IXLWorksheet worksheet, int row, string title, string locale)
    {
        worksheet.Cell(row, 1).Value = title;
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Font.FontSize = ExcelStyles.TitleFontSize;
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        row++;

        return row;
    }

    private int AddUserInfo(IXLWorksheet worksheet, int row, UserLoadReportDto report, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("User Name", locale) + ":";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.UserName;
        row++;

        worksheet.Cell(row, 1).Value = "Email:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.UserEmail;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Agency", locale) + ":";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.AgencyName;
        row++;

        return row;
    }

    private int AddPeriodInfo(IXLWorksheet worksheet, int row, DateTime fromDate, DateTime toDate, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Period", locale) + ":";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = $"{fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy}";
        row++;

        return row;
    }

    private int AddUserStatistics(IXLWorksheet worksheet, int row, UserLoadReportDto report, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Working Days", locale) + ":";
        worksheet.Cell(row, 2).Value = report.WorkingDaysCount;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Average Hours Per Day", locale) + ":";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerDay;
        row++;

        return row;
    }

    private int AddDailyBreakdown(IXLWorksheet worksheet, int row, List<DailyBreakdownDto> breakdown, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Daily Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Date", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Day of Week", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Hours", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Entries Count", locale);

        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var day in breakdown)
        {
            worksheet.Cell(row, 1).Value = day.Date.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 2).Value = day.DayOfWeek;
            worksheet.Cell(row, 3).Value = day.Hours;
            worksheet.Cell(row, 4).Value = day.EntriesCount;

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientBreakdown(IXLWorksheet worksheet, int row, List<ClientBreakdownDto> breakdown, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var client in breakdown)
        {
            worksheet.Cell(row, 1).Value = client.ClientName;
            worksheet.Cell(row, 2).Value = client.TotalHours;
            worksheet.Cell(row, 3).Value = client.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{client.Percentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddJobTypeBreakdown(IXLWorksheet worksheet, int row, List<JobTypeBreakdownDto> breakdown, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Job Type Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Job Type", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var jobType in breakdown)
        {
            worksheet.Cell(row, 1).Value = jobType.JobTypeName;
            worksheet.Cell(row, 2).Value = jobType.TotalHours;
            worksheet.Cell(row, 3).Value = jobType.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{jobType.Percentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddProjectBreakdown(IXLWorksheet worksheet, int row, List<ProjectBreakdownDto> breakdown, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Project Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Project", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var project in breakdown)
        {
            worksheet.Cell(row, 1).Value = project.ProjectBrandName;
            worksheet.Cell(row, 2).Value = project.TotalHours;
            worksheet.Cell(row, 3).Value = project.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{project.Percentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private string GetLocalizedText(string key, string locale)
    {
        var isUkrainian = locale.ToLower() == Locales.Ukrainian;

        return key switch
        {
            "User Load Report" => isUkrainian ? HeadersUk.UserLoadReport : HeadersEn.UserLoadReport,
            "Team Load Report" => isUkrainian ? HeadersUk.TeamLoadReport : HeadersEn.TeamLoadReport,
            "Client Report" => isUkrainian ? HeadersUk.ClientReport : HeadersEn.ClientReport,
            "Time Summary Report" => isUkrainian ? HeadersUk.TimeSummaryReport : HeadersEn.TimeSummaryReport,
            "Summary Report" => isUkrainian ? "Зведений звіт" : "Summary Report",

            "User Name" => isUkrainian ? HeadersUk.UserName : HeadersEn.UserName,
            "User ID" => isUkrainian ? HeadersUk.UserId : HeadersEn.UserId,
            "Email" => "Email",
            "Agency" => isUkrainian ? HeadersUk.AgencyName : HeadersEn.AgencyName,

            "Date" => isUkrainian ? HeadersUk.Date : HeadersEn.Date,
            "From Date" => isUkrainian ? HeadersUk.FromDate : HeadersEn.FromDate,
            "To Date" => isUkrainian ? HeadersUk.ToDate : HeadersEn.ToDate,
            "Period" => isUkrainian ? HeadersUk.Period : HeadersEn.Period,
            "Day of Week" => isUkrainian ? "День тижня" : "Day of Week",

            "Hours" => isUkrainian ? HeadersUk.Hours : HeadersEn.Hours,
            "Total Hours" => isUkrainian ? HeadersUk.TotalHours : HeadersEn.TotalHours,
            "Average Hours" => isUkrainian ? HeadersUk.AverageHours : HeadersEn.AverageHours,
            "Average Hours Per Day" => isUkrainian ? "Середньо годин на день" : "Average Hours Per Day",
            "Working Days" => isUkrainian ? HeadersUk.WorkingDays : HeadersEn.WorkingDays,

            "Client" => isUkrainian ? HeadersUk.Client : HeadersEn.Client,
            "Project" => isUkrainian ? HeadersUk.Project : HeadersEn.Project,
            "Job Type" => isUkrainian ? HeadersUk.JobType : HeadersEn.JobType,
            "Media" => isUkrainian ? HeadersUk.Media : HeadersEn.Media,

            "Entries Count" => isUkrainian ? HeadersUk.EntriesCount : HeadersEn.EntriesCount,
            "Percentage" => isUkrainian ? HeadersUk.Percentage : HeadersEn.Percentage,
            "Total" => isUkrainian ? HeadersUk.Total : HeadersEn.Total,
            "Statistics" => isUkrainian ? "Статистика" : "Statistics",
            "Daily Breakdown" => isUkrainian ? "Деталізація по днях" : "Daily Breakdown",
            "Client Breakdown" => isUkrainian ? "Деталізація по клієнтах" : "Client Breakdown",
            "Job Type Breakdown" => isUkrainian ? "Деталізація по типах робіт" : "Job Type Breakdown",
            "Project Breakdown" => isUkrainian ? "Деталізація по проєктах" : "Project Breakdown",
            "Members Load" => isUkrainian ? "Навантаження членів команди" : "Members Load",
            "Top Clients" => isUkrainian ? "Топ клієнти" : "Top Clients",
            "Top Users" => isUkrainian ? "Топ користувачі" : "Top Users",

            "Total Members" => isUkrainian ? "Всього членів" : "Total Members",
            "Active Members" => isUkrainian ? "Активних членів" : "Active Members",
            "Average Hours Per Member" => isUkrainian ? "Середньо годин на члена" : "Average Hours Per Member",

            "Unique Users" => isUkrainian ? "Унікальних користувачів" : "Unique Users",
            "Unique Projects" => isUkrainian ? "Унікальних проєктів" : "Unique Projects",

            "Team Statistics" => isUkrainian ? "Статистика команди" : "Team Statistics",
            "Total Team Hours" => isUkrainian ? "Всього годин команди" : "Total Team Hours",
            "Load Percentage" => isUkrainian ? "Відсоток навантаження" : "Load Percentage",
            "Projects Count" => isUkrainian ? "Кількість проєктів" : "Projects Count",
            "Users Count" => isUkrainian ? "Кількість користувачів" : "Users Count",

            "Overall Statistics" => isUkrainian ? "Загальна статистика" : "Overall Statistics",
            "Total Users" => isUkrainian ? "Всього користувачів" : "Total Users",
            "Total Clients" => isUkrainian ? "Всього клієнтів" : "Total Clients",
            "Total Projects" => isUkrainian ? "Всього проєктів" : "Total Projects",

            _ => key
        };
    }

    private int AddTeamStatistics(IXLWorksheet worksheet, int row, TeamLoadReportDto report, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Team Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Team Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalTeamHours;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Members", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalMembers;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Active Members", locale) + ":";
        worksheet.Cell(row, 2).Value = report.ActiveMembers;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Average Hours Per Member", locale) + ":";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerMember;
        row++;

        return row;
    }

    private int AddMembersLoad(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> members, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Members Load", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 6).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("User Name", locale);
        worksheet.Cell(row, 2).Value = "Email";
        worksheet.Cell(row, 3).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Working Days", locale);
        worksheet.Cell(row, 6).Value = GetLocalizedText("Load Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var member in members)
        {
            worksheet.Cell(row, 1).Value = member.UserName;
            worksheet.Cell(row, 2).Value = member.UserEmail;
            worksheet.Cell(row, 3).Value = member.TotalHours;
            worksheet.Cell(row, 4).Value = member.EntriesCount;
            worksheet.Cell(row, 5).Value = member.WorkingDays;
            worksheet.Cell(row, 6).Value = $"{member.LoadPercentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

// Виправлена версія - для ClientBreakdownDto
    private int AddTopClientsFromClientBreakdown(IXLWorksheet worksheet, int row, List<ClientBreakdownDto> clients,
        string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Top Clients", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var client in clients)
        {
            worksheet.Cell(row, 1).Value = client.ClientName;
            worksheet.Cell(row, 2).Value = client.TotalHours;
            worksheet.Cell(row, 3).Value = client.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{client.Percentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

// Нова версія - для ClientSummaryDto
    private int AddTopClientsFromClientSummary(IXLWorksheet worksheet, int row, List<ClientSummaryDto> clients,
        string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Top Clients", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Projects Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Users Count", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var client in clients)
        {
            worksheet.Cell(row, 1).Value = client.ClientName;
            worksheet.Cell(row, 2).Value = client.TotalHours;
            worksheet.Cell(row, 3).Value = client.ProjectsCount;
            worksheet.Cell(row, 4).Value = client.UsersCount;
            worksheet.Cell(row, 5).Value = $"{client.Percentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientStatistics(IXLWorksheet worksheet, int row, ClientReportDto report, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Unique Users", locale) + ":";
        worksheet.Cell(row, 2).Value = report.UniqueUsers;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Unique Projects", locale) + ":";
        worksheet.Cell(row, 2).Value = report.UniqueProjects;
        row++;

        return row;
    }

    private int AddSummaryStatistics(IXLWorksheet worksheet, int row, TimeSummaryReportDto report, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Overall Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Users", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalUsers;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Clients", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalClients;
        row++;

        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Projects", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalProjects;
        row++;

        return row;
    }

    private int AddTopUsers(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> users, string locale)
    {
        worksheet.Cell(row, 1).Value = GetLocalizedText("Top Users", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("User Name", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Working Days", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Load Percentage", locale);

        var headerRange = worksheet.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        foreach (var user in users)
        {
            worksheet.Cell(row, 1).Value = user.UserName;
            worksheet.Cell(row, 2).Value = user.TotalHours;
            worksheet.Cell(row, 3).Value = user.EntriesCount;
            worksheet.Cell(row, 4).Value = user.WorkingDays;
            worksheet.Cell(row, 5).Value = $"{user.LoadPercentage:F2}%";

            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private byte[] ExportToCsvInternal<T>(IEnumerable<T> data, string locale) where T : class
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        });

        csv.WriteRecords(data);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private class UserLoadCsvRecord
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
        public int EntriesCount { get; set; }
    }

    private class TeamLoadCsvRecord
    {
        public string AgencyName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TotalHours { get; set; } = string.Empty;
        public int EntriesCount { get; set; }
        public int WorkingDays { get; set; }
        public string AverageHoursPerDay { get; set; } = string.Empty;
        public double LoadPercentage { get; set; }
    }

    private class ClientReportCsvRecord
    {
        public string ClientName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string TotalHours { get; set; } = string.Empty;
        public int EntriesCount { get; set; }
        public double Percentage { get; set; }
    }

    private class TimeSummaryCsvRecord
    {
        public string ClientName { get; set; } = string.Empty;
        public string TotalHours { get; set; } = string.Empty;
        public int ProjectsCount { get; set; }
        public int UsersCount { get; set; }
        public double Percentage { get; set; }
    }

    public async Task<byte[]> ExportUserEntriesFlatToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        ExportColumnsDto columns,
        string locale = "uk")
    {
        try
        {
            var entries = await _reportService.GetUserTimeEntriesForExportAsync(
                userId, fromDate, toDate, requestingUserId);

            var entriesList = entries.ToList();
            var userName = entriesList.FirstOrDefault()?.UserName ?? userId.ToString();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            // Динамически строим заголовки и запоминаем номер колонки
            var col = 1;
            var colMap = new Dictionary<string, int>();

            void AddHeader(string key, string label)
            {
                colMap[key] = col;
                var cell = worksheet.Cell(1, col);
                cell.Value = label;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
                cell.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                col++;
            }

            if (columns.Agency) AddHeader("agency", GetLocalizedText("Agency", locale));
            if (columns.FullName) AddHeader("fullname", GetLocalizedText("Name", locale));
            if (columns.Date) AddHeader("date", GetLocalizedText("Date", locale));
            if (columns.Market) AddHeader("market", GetLocalizedText("Market", locale));
            if (columns.ContractingAgency)
                AddHeader("contractingagency", GetLocalizedText("Contracting Agency", locale));
            if (columns.Client) AddHeader("client", GetLocalizedText("Client", locale));
            if (columns.ProjectBrand) AddHeader("projectbrand", GetLocalizedText("Project / Brand", locale));
            if (columns.Media) AddHeader("media", GetLocalizedText("Media", locale));
            if (columns.JobType) AddHeader("jobtype", GetLocalizedText("Job Type", locale));
            if (columns.Hours) AddHeader("hours", GetLocalizedText("Hours", locale));
            if (columns.Comments) AddHeader("comments", GetLocalizedText("Comments", locale));

            var row = 2;
            foreach (var entry in entriesList)
            {
                if (columns.Agency) worksheet.Cell(row, colMap["agency"]).Value = entry.AgencyName;
                if (columns.FullName) worksheet.Cell(row, colMap["fullname"]).Value = entry.UserName;
                if (columns.Date) worksheet.Cell(row, colMap["date"]).Value = entry.EntryDate.ToString("dd.MM.yyyy");
                if (columns.Market) worksheet.Cell(row, colMap["market"]).Value = entry.MarketName;
                if (columns.ContractingAgency)
                    worksheet.Cell(row, colMap["contractingagency"]).Value = entry.ContractingAgencyName;
                if (columns.Client) worksheet.Cell(row, colMap["client"]).Value = entry.ClientName;
                if (columns.ProjectBrand) worksheet.Cell(row, colMap["projectbrand"]).Value = entry.ProjectBrandName;
                if (columns.Media) worksheet.Cell(row, colMap["media"]).Value = entry.MediaName;
                if (columns.JobType) worksheet.Cell(row, colMap["jobtype"]).Value = entry.JobTypeName;
                if (columns.Hours)
                    worksheet.Cell(row, colMap["hours"]).Value =
                        Math.Round((double)entry.HoursMilliseconds / 3600000, 2);
                if (columns.Comments) worksheet.Cell(row, colMap["comments"]).Value = entry.Comments ?? string.Empty;

                // Чередование цветов строк
                if (row % 2 == 0)
                {
                    worksheet.Range(row, 1, row, col - 1).Style.Fill.BackgroundColor =
                        XLColor.FromHtml(ExcelStyles.AlternateRowColor);
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            await _auditService.LogReportExportedAsync(
                reportType: "UserEntriesFlat",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: userName,
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: true);

            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при flat-експорті записів користувача {UserId}", userId);

            await _auditService.LogReportExportedAsync(
                reportType: "UserEntriesFlat",
                exportFormat: "Excel",
                requestingUserId: requestingUserId,
                requestingUserName: "Unknown",
                reportParams: new { UserId = userId, FromDate = fromDate, ToDate = toDate },
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: false,
                errorMessage: ex.Message);

            throw;
        }
    }
    
    public async Task<byte[]> ExportAllEntriesFlatToExcelAsync(
    DateTime fromDate,
    DateTime toDate,
    long requestingUserId,
    string ipAddress,
    string userAgent,
    ExportColumnsDto columns,
    string locale = "uk")
{
    try
    {
        var entries = await _reportService.GetAllTimeEntriesForExportAsync(
            fromDate, toDate, requestingUserId);

        var entriesList = entries.ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        var col = 1;
        var colMap = new Dictionary<string, int>();

        void AddHeader(string key, string label)
        {
            colMap[key] = col;
            var cell = worksheet.Cell(1, col);
            cell.Value = label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
            cell.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
        }

        if (columns.Agency) AddHeader("agency", GetLocalizedText("Agency", locale));
        if (columns.FullName) AddHeader("fullname", GetLocalizedText("Name", locale));
        if (columns.Date) AddHeader("date", GetLocalizedText("Date", locale));
        if (columns.Market) AddHeader("market", GetLocalizedText("Market", locale));
        if (columns.ContractingAgency) AddHeader("contractingagency", GetLocalizedText("Contracting Agency", locale));
        if (columns.Client) AddHeader("client", GetLocalizedText("Client", locale));
        if (columns.ProjectBrand) AddHeader("projectbrand", GetLocalizedText("Project / Brand", locale));
        if (columns.Media) AddHeader("media", GetLocalizedText("Media", locale));
        if (columns.JobType) AddHeader("jobtype", GetLocalizedText("Job Type", locale));
        if (columns.Hours) AddHeader("hours", GetLocalizedText("Hours", locale));
        if (columns.Comments) AddHeader("comments", GetLocalizedText("Comments", locale));

        var row = 2;
        foreach (var entry in entriesList)
        {
            if (columns.Agency) worksheet.Cell(row, colMap["agency"]).Value = entry.AgencyName;
            if (columns.FullName) worksheet.Cell(row, colMap["fullname"]).Value = entry.UserName;
            if (columns.Date) worksheet.Cell(row, colMap["date"]).Value = entry.EntryDate.ToString("dd.MM.yyyy");
            if (columns.Market) worksheet.Cell(row, colMap["market"]).Value = entry.MarketName;
            if (columns.ContractingAgency) worksheet.Cell(row, colMap["contractingagency"]).Value = entry.ContractingAgencyName;
            if (columns.Client) worksheet.Cell(row, colMap["client"]).Value = entry.ClientName;
            if (columns.ProjectBrand) worksheet.Cell(row, colMap["projectbrand"]).Value = entry.ProjectBrandName;
            if (columns.Media) worksheet.Cell(row, colMap["media"]).Value = entry.MediaName;
            if (columns.JobType) worksheet.Cell(row, colMap["jobtype"]).Value = entry.JobTypeName;
            if (columns.Hours) worksheet.Cell(row, colMap["hours"]).Value =
                Math.Round((double)entry.HoursMilliseconds / 3600000, 2);
            if (columns.Comments) worksheet.Cell(row, colMap["comments"]).Value = entry.Comments ?? string.Empty;

            if (row % 2 == 0)
                worksheet.Range(row, 1, row, col - 1).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        await _auditService.LogReportExportedAsync(
            reportType: "AllEntriesFlat",
            exportFormat: "Excel",
            requestingUserId: requestingUserId,
            requestingUserName: "Admin",
            reportParams: new { FromDate = fromDate, ToDate = toDate },
            ipAddress: ipAddress,
            userAgent: userAgent,
            success: true);

        return stream.ToArray();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Помилка при flat-експорті всіх записів");

        await _auditService.LogReportExportedAsync(
            reportType: "AllEntriesFlat",
            exportFormat: "Excel",
            requestingUserId: requestingUserId,
            requestingUserName: "Unknown",
            reportParams: new { FromDate = fromDate, ToDate = toDate },
            ipAddress: ipAddress,
            userAgent: userAgent,
            success: false,
            errorMessage: ex.Message);

        throw;
    }
}
}