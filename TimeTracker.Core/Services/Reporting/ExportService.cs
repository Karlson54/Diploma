using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Reports;
using TimeTracker.Core.DTOs.Reports.Common;
using TimeTracker.Core.Services.Audit;
using static TimeTracker.Core.Common.ExportConstants;

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
        string userAgent)
    {
        try
        {
            var report = await _reportService.GetUserLoadReportAsync(
                userId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("User Load Report");

            var currentRow = 1;

            currentRow = AddReportTitle(worksheet, currentRow, "User Load Report");
            currentRow = AddUserInfo(worksheet, currentRow, report);
            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate);
            currentRow++;

            currentRow = AddUserStatistics(worksheet, currentRow, report);
            currentRow++;

            if (report.DailyBreakdown.Any())
            {
                currentRow = AddDailyBreakdown(worksheet, currentRow, report.DailyBreakdown);
                currentRow++;
            }

            if (report.ClientBreakdown.Any())
            {
                currentRow = AddClientBreakdown(worksheet, currentRow, report.ClientBreakdown);
                currentRow++;
            }

            if (report.JobTypeBreakdown.Any())
            {
                currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

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
            _logger.LogError(ex, "Помилка при експорті User Load Report для користувача {UserId}", userId);

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
        string userAgent)
    {
        try
        {
            var report = await _reportService.GetTeamLoadReportAsync(
                agencyId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Team Load Report");

            var currentRow = 1;

            currentRow = AddReportTitle(worksheet, currentRow, "Team Load Report");

            worksheet.Cell(currentRow, 1).Value = "Agency:";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 2).Value = report.AgencyName;
            currentRow++;

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate);
            currentRow++;

            currentRow = AddTeamStatistics(worksheet, currentRow, report);
            currentRow++;

            if (report.MembersLoad.Any())
            {
                currentRow = AddMembersLoad(worksheet, currentRow, report.MembersLoad);
                currentRow++;
            }

            if (report.TopClients.Any())
            {
                currentRow = AddTopClientsFromClientBreakdown(worksheet, currentRow, report.TopClients);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

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
            _logger.LogError(ex, "Помилка при експорті Team Load Report для агентства {AgencyId}", agencyId);

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
        string userAgent)
    {
        try
        {
            var report = await _reportService.GetClientReportAsync(
                clientId, fromDate, toDate, requestingUserId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Client Report");

            var currentRow = 1;

            currentRow = AddReportTitle(worksheet, currentRow, "Client Report");

            worksheet.Cell(currentRow, 1).Value = "Client:";
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

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate);
            currentRow++;

            currentRow = AddClientStatistics(worksheet, currentRow, report);
            currentRow++;

            if (report.ProjectBreakdown.Any())
            {
                currentRow = AddProjectBreakdown(worksheet, currentRow, report.ProjectBreakdown);
                currentRow++;
            }

            if (report.JobTypeBreakdown.Any())
            {
                currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

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
            _logger.LogError(ex, "Помилка при експорті Client Report для клієнта {ClientId}", clientId);

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
        long? clientId = null)
    {
        try
        {
            var report = await _reportService.GetTimeSummaryReportAsync(
                fromDate, toDate, requestingUserId, agencyId, clientId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Time Summary Report");

            var currentRow = 1;

            currentRow = AddReportTitle(worksheet, currentRow, "Time Summary Report");

            if (agencyId.HasValue)
            {
                worksheet.Cell(currentRow, 1).Value = "Agency:";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = report.AgencyName ?? "";
                currentRow++;
            }

            if (clientId.HasValue)
            {
                worksheet.Cell(currentRow, 1).Value = "Client:";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 2).Value = report.ClientName ?? "";
                currentRow++;
            }

            currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate);
            currentRow++;

            currentRow = AddSummaryStatistics(worksheet, currentRow, report);
            currentRow++;

            if (report.TopUsers.Any())
            {
                currentRow = AddTopUsers(worksheet, currentRow, report.TopUsers);
                currentRow++;
            }

            if (report.TopClients.Any())
            {
                currentRow = AddTopClientsFromClientSummary(worksheet, currentRow, report.TopClients);
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

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
            _logger.LogError(ex, "Помилка при експорті Time Summary Report");

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

    public Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string sheetName,
        string? title = null,
        bool applyFormatting = true) where T : class
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var dataList = data.ToList();
        if (!dataList.Any())
            return Task.FromResult(Array.Empty<byte>());

        var currentRow = 1;

        if (!string.IsNullOrEmpty(title))
        {
            worksheet.Cell(currentRow, 1).Value = title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = ExcelStyles.TitleFontSize;
            currentRow += 2;
        }

        var table = worksheet.Cell(currentRow, 1).InsertTable(dataList);

        if (applyFormatting)
        {
            var headerRow = table.HeadersRow();
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
            headerRow.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);

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

    private int AddReportTitle(IXLWorksheet worksheet, int row, string title)
    {
        worksheet.Cell(row, 1).Value = title;
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Font.FontSize = ExcelStyles.TitleFontSize;
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        row++;
        return row;
    }

    private int AddUserInfo(IXLWorksheet worksheet, int row, UserLoadReportDto report)
    {
        worksheet.Cell(row, 1).Value = "User Name:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.UserName;
        row++;

        worksheet.Cell(row, 1).Value = "Email:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.UserEmail;
        row++;

        worksheet.Cell(row, 1).Value = "Agency:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = report.AgencyName;
        row++;

        return row;
    }

    private int AddPeriodInfo(IXLWorksheet worksheet, int row, DateTime fromDate, DateTime toDate)
    {
        worksheet.Cell(row, 1).Value = "Period:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = $"{fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy}";
        row++;
        return row;
    }

    private int AddUserStatistics(IXLWorksheet worksheet, int row, UserLoadReportDto report)
    {
        worksheet.Cell(row, 1).Value = "Statistics";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = "Total Hours:";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = "Entries Count:";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = "Working Days:";
        worksheet.Cell(row, 2).Value = report.WorkingDaysCount;
        row++;

        worksheet.Cell(row, 1).Value = "Average Hours Per Day:";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerDay;
        row++;

        return row;
    }

    private int AddDailyBreakdown(IXLWorksheet worksheet, int row, List<DailyBreakdownDto> breakdown)
    {
        worksheet.Cell(row, 1).Value = "Daily Breakdown";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 4);
        worksheet.Cell(row, 1).Value = "Date";
        worksheet.Cell(row, 2).Value = "Day of Week";
        worksheet.Cell(row, 3).Value = "Hours";
        worksheet.Cell(row, 4).Value = "Entries Count";
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
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientBreakdown(IXLWorksheet worksheet, int row, List<ClientBreakdownDto> breakdown)
    {
        worksheet.Cell(row, 1).Value = "Client Breakdown";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 4);
        worksheet.Cell(row, 1).Value = "Client";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Entries Count";
        worksheet.Cell(row, 4).Value = "Percentage (%)";
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
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddJobTypeBreakdown(IXLWorksheet worksheet, int row, List<JobTypeBreakdownDto> breakdown)
    {
        worksheet.Cell(row, 1).Value = "Job Type Breakdown";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 4);
        worksheet.Cell(row, 1).Value = "Job Type";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Entries Count";
        worksheet.Cell(row, 4).Value = "Percentage (%)";
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
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddProjectBreakdown(IXLWorksheet worksheet, int row, List<ProjectBreakdownDto> breakdown)
    {
        worksheet.Cell(row, 1).Value = "Project Breakdown";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 4);
        worksheet.Cell(row, 1).Value = "Project";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Entries Count";
        worksheet.Cell(row, 4).Value = "Percentage (%)";
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
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddTeamStatistics(IXLWorksheet worksheet, int row, TeamLoadReportDto report)
    {
        worksheet.Cell(row, 1).Value = "Team Statistics";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = "Total Team Hours:";
        worksheet.Cell(row, 2).Value = report.TotalTeamHours;
        row++;

        worksheet.Cell(row, 1).Value = "Total Members:";
        worksheet.Cell(row, 2).Value = report.TotalMembers;
        row++;

        worksheet.Cell(row, 1).Value = "Active Members:";
        worksheet.Cell(row, 2).Value = report.ActiveMembers;
        row++;

        worksheet.Cell(row, 1).Value = "Average Hours Per Member:";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerMember;
        row++;

        return row;
    }

    private int AddMembersLoad(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> members)
    {
        worksheet.Cell(row, 1).Value = "Members Load";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 6).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 6);
        worksheet.Cell(row, 1).Value = "User Name";
        worksheet.Cell(row, 2).Value = "Email";
        worksheet.Cell(row, 3).Value = "Total Hours";
        worksheet.Cell(row, 4).Value = "Entries Count";
        worksheet.Cell(row, 5).Value = "Working Days";
        worksheet.Cell(row, 6).Value = "Load (%)";
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
                worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

// Виправлена версія - для ClientBreakdownDto
    private int AddTopClientsFromClientBreakdown(IXLWorksheet worksheet, int row, List<ClientBreakdownDto> clients)
    {
        worksheet.Cell(row, 1).Value = "Top Clients";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 4);
        worksheet.Cell(row, 1).Value = "Client";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Entries Count";
        worksheet.Cell(row, 4).Value = "Percentage (%)";
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
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

// Нова версія - для ClientSummaryDto
    private int AddTopClientsFromClientSummary(IXLWorksheet worksheet, int row, List<ClientSummaryDto> clients)
    {
        worksheet.Cell(row, 1).Value = "Top Clients";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 5);
        worksheet.Cell(row, 1).Value = "Client";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Projects Count";
        worksheet.Cell(row, 4).Value = "Users Count";
        worksheet.Cell(row, 5).Value = "Percentage (%)";
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
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientStatistics(IXLWorksheet worksheet, int row, ClientReportDto report)
    {
        worksheet.Cell(row, 1).Value = "Statistics";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = "Total Hours:";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = "Entries Count:";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = "Unique Users:";
        worksheet.Cell(row, 2).Value = report.UniqueUsers;
        row++;

        worksheet.Cell(row, 1).Value = "Unique Projects:";
        worksheet.Cell(row, 2).Value = report.UniqueProjects;
        row++;

        return row;
    }

    private int AddSummaryStatistics(IXLWorksheet worksheet, int row, TimeSummaryReportDto report)
    {
        worksheet.Cell(row, 1).Value = "Overall Statistics";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        worksheet.Cell(row, 1).Value = "Total Hours:";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        worksheet.Cell(row, 1).Value = "Entries Count:";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        worksheet.Cell(row, 1).Value = "Total Users:";
        worksheet.Cell(row, 2).Value = report.TotalUsers;
        row++;

        worksheet.Cell(row, 1).Value = "Total Clients:";
        worksheet.Cell(row, 2).Value = report.TotalClients;
        row++;

        worksheet.Cell(row, 1).Value = "Total Projects:";
        worksheet.Cell(row, 2).Value = report.TotalProjects;
        row++;

        return row;
    }

    private int AddTopUsers(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> users)
    {
        worksheet.Cell(row, 1).Value = "Top Users";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        var headerRow = row;
        var headerRange = worksheet.Range(row, 1, row, 5);
        worksheet.Cell(row, 1).Value = "User Name";
        worksheet.Cell(row, 2).Value = "Total Hours";
        worksheet.Cell(row, 3).Value = "Entries Count";
        worksheet.Cell(row, 4).Value = "Working Days";
        worksheet.Cell(row, 5).Value = "Load (%)";
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
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);

            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    public async Task<byte[]> ExportUserEntriesFlatToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string ipAddress,
        string userAgent,
        ExportColumnsDto columns)
    {
        try
        {
            var entries = await _reportService.GetUserTimeEntriesForExportAsync(
                userId, fromDate, toDate, requestingUserId);

            var entriesList = entries.ToList();
            var userName = entriesList.FirstOrDefault()?.UserName ?? userId.ToString();

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

            if (columns.Agency) AddHeader("agency", "Agency");
            if (columns.Department) AddHeader("department", "Department");
            if (columns.FullName) AddHeader("fullname", "Name");
            if (columns.Date) AddHeader("date", "Date");
            if (columns.Month) AddHeader("month", "Month");
            if (columns.Year) AddHeader("year", "Year");
            if (columns.Market) AddHeader("market", "Market");
            if (columns.ContractingAgency) AddHeader("contractingagency", "Contracting Agency");
            if (columns.Client) AddHeader("client", "Client");
            if (columns.ProjectBrand) AddHeader("projectbrand", "Project / Brand");
            if (columns.Media) AddHeader("media", "Media");
            if (columns.JobType) AddHeader("jobtype", "Job Type");
            if (columns.Hours) AddHeader("hours", "Hours");
            if (columns.Comments) AddHeader("comments", "Comments");

            var row = 2;
            foreach (var entry in entriesList)
            {
                if (columns.Agency) worksheet.Cell(row, colMap["agency"]).Value = entry.AgencyName;
                if (columns.Department) worksheet.Cell(row, colMap["department"]).Value = entry.DepartmentName ?? "-";
                if (columns.FullName) worksheet.Cell(row, colMap["fullname"]).Value = entry.UserName;
                if (columns.Date) worksheet.Cell(row, colMap["date"]).Value = entry.EntryDate.ToString("dd.MM.yyyy");
                if (columns.Month)
                    worksheet.Cell(row, colMap["month"]).Value =
                        entry.EntryDate.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                if (columns.Year) worksheet.Cell(row, colMap["year"]).Value = entry.EntryDate.Year;
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
        ExportColumnsDto columns)
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

            if (columns.Agency) AddHeader("agency", "Agency");
            if (columns.Department) AddHeader("department", "Department");
            if (columns.FullName) AddHeader("fullname", "Name");
            if (columns.Date) AddHeader("date", "Date");
            if (columns.Month) AddHeader("month", "Month");
            if (columns.Year) AddHeader("year", "Year");
            if (columns.Market) AddHeader("market", "Market");
            if (columns.ContractingAgency) AddHeader("contractingagency", "Contracting Agency");
            if (columns.Client) AddHeader("client", "Client");
            if (columns.ProjectBrand) AddHeader("projectbrand", "Project / Brand");
            if (columns.Media) AddHeader("media", "Media");
            if (columns.JobType) AddHeader("jobtype", "Job Type");
            if (columns.Hours) AddHeader("hours", "Hours");
            if (columns.Comments) AddHeader("comments", "Comments");

            var row = 2;
            foreach (var entry in entriesList)
            {
                if (columns.Agency) worksheet.Cell(row, colMap["agency"]).Value = entry.AgencyName;
                if (columns.Department) worksheet.Cell(row, colMap["department"]).Value = entry.DepartmentName ?? "-";
                if (columns.FullName) worksheet.Cell(row, colMap["fullname"]).Value = entry.UserName;
                if (columns.Date) worksheet.Cell(row, colMap["date"]).Value = entry.EntryDate.ToString("dd.MM.yyyy");
                if (columns.Month)
                    worksheet.Cell(row, colMap["month"]).Value =
                        entry.EntryDate.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
                if (columns.Year) worksheet.Cell(row, colMap["year"]).Value = entry.EntryDate.Year;
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