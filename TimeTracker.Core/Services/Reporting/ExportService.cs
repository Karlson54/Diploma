using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Reports;
using TimeTracker.Core.DTOs.Reports.Common;
using static TimeTracker.Core.Common.ExportConstants;

namespace TimeTracker.Core.Services.Reporting;

public class ExportService : IExportService
{
    private readonly IReportService _reportService;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IReportService reportService,
        ILogger<ExportService> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    #region Excel Export Methods

    public async Task<byte[]> ExportUserLoadReportToExcelAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string locale = "uk")
    {
        // Отримуємо дані звіту
        var report = await _reportService.GetUserLoadReportAsync(
            userId, fromDate, toDate, requestingUserId);

        // Створюємо Excel файл
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetLocalizedText("User Load Report", locale));

        var currentRow = 1;

        // Заголовок звіту
        currentRow = AddReportTitle(
            worksheet, 
            currentRow, 
            GetLocalizedText("User Load Report", locale),
            locale);

        // Інфо про користувача
        currentRow = AddUserInfo(worksheet, currentRow, report, locale);

        // Інфо про період
        currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);

        currentRow++; // Пустий рядок

        // Загальна статистика
        currentRow = AddUserStatistics(worksheet, currentRow, report, locale);

        currentRow++; // Пустий рядок

        // Деталізація по днях
        if (report.DailyBreakdown.Any())
        {
            currentRow = AddDailyBreakdown(worksheet, currentRow, report.DailyBreakdown, locale);
            currentRow++; // Пустий рядок
        }

        // Деталізація по клієнтах
        if (report.ClientBreakdown.Any())
        {
            currentRow = AddClientBreakdown(worksheet, currentRow, report.ClientBreakdown, locale);
            currentRow++; // Пустий рядок
        }

        // Деталізація по типах робіт
        if (report.JobTypeBreakdown.Any())
        {
            currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown, locale);
        }

        // Автоширина колонок
        worksheet.Columns().AdjustToContents();

        // Зберігаємо в пам'ять
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        
        _logger.LogInformation(
            "Експортовано User Load Report для користувача {UserId} у форматі Excel",
            userId);

        return stream.ToArray();
    }

    public async Task<byte[]> ExportTeamLoadReportToExcelAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string locale = "uk")
    {
        var report = await _reportService.GetTeamLoadReportAsync(
            agencyId, fromDate, toDate, requestingUserId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetLocalizedText("Team Load Report", locale));

        var currentRow = 1;

        // Заголовок
        currentRow = AddReportTitle(
            worksheet,
            currentRow,
            GetLocalizedText("Team Load Report", locale),
            locale);

        // Інфо про агентство
        worksheet.Cell(currentRow, 1).Value = GetLocalizedText("Agency", locale) + ":";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 2).Value = report.AgencyName;
        currentRow++;

        // Період
        currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
        currentRow++;

        // Статистика команди
        currentRow = AddTeamStatistics(worksheet, currentRow, report, locale);
        currentRow++;

        // Навантаження членів команди
        if (report.MembersLoad.Any())
        {
            currentRow = AddMembersLoad(worksheet, currentRow, report.MembersLoad, locale);
            currentRow++;
        }

        // Топ клієнти
        if (report.TopClients.Any())
        {
            currentRow = AddTopClients(worksheet, currentRow, report.TopClients, locale);
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger.LogInformation(
            "Експортовано Team Load Report для агентства {AgencyId} у форматі Excel",
            agencyId);

        return stream.ToArray();
    }

    public async Task<byte[]> ExportClientReportToExcelAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        string locale = "uk")
    {
        var report = await _reportService.GetClientReportAsync(
            clientId, fromDate, toDate, requestingUserId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetLocalizedText("Client Report", locale));

        var currentRow = 1;

        // Заголовок
        currentRow = AddReportTitle(
            worksheet,
            currentRow,
            GetLocalizedText("Client Report", locale),
            locale);

        // Інфо про клієнта
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

        // Період
        currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
        currentRow++;

        // Статистика
        currentRow = AddClientStatistics(worksheet, currentRow, report, locale);
        currentRow++;

        // Деталізація по проєктах
        if (report.ProjectBreakdown.Any())
        {
            currentRow = AddProjectBreakdown(worksheet, currentRow, report.ProjectBreakdown, locale);
            currentRow++;
        }

        // Деталізація по типах робіт
        if (report.JobTypeBreakdown.Any())
        {
            currentRow = AddJobTypeBreakdown(worksheet, currentRow, report.JobTypeBreakdown, locale);
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger.LogInformation(
            "Експортовано Client Report для клієнта {ClientId} у форматі Excel",
            clientId);

        return stream.ToArray();
    }

    public async Task<byte[]> ExportTimeSummaryReportToExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        long? agencyId = null,
        long? clientId = null,
        string locale = "uk")
    {
        var report = await _reportService.GetTimeSummaryReportAsync(
            fromDate, toDate, requestingUserId, agencyId, clientId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetLocalizedText("Summary Report", locale));

        var currentRow = 1;

        // Заголовок
        currentRow = AddReportTitle(
            worksheet,
            currentRow,
            GetLocalizedText("Time Summary Report", locale),
            locale);

        // Фільтри
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

        // Період
        currentRow = AddPeriodInfo(worksheet, currentRow, report.FromDate, report.ToDate, locale);
        currentRow++;

        // Загальна статистика
        currentRow = AddSummaryStatistics(worksheet, currentRow, report, locale);
        currentRow++;

        // Топ користувачі
        if (report.TopUsers.Any())
        {
            currentRow = AddTopUsers(worksheet, currentRow, report.TopUsers, locale);
            currentRow++;
        }

        // Топ клієнти
        if (report.TopClients.Any())
        {
            currentRow = AddTopClients(worksheet, currentRow, report.TopClients, locale);
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger.LogInformation(
            "Експортовано Time Summary Report у форматі Excel");

        return stream.ToArray();
    }

    #endregion

    #region Helper Methods for Excel Formatting

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
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        // Всього годин
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        // Всього записів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        // Робочих днів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Working Days", locale) + ":";
        worksheet.Cell(row, 2).Value = report.WorkingDaysCount;
        row++;

        // Середньо годин на день
        worksheet.Cell(row, 1).Value = GetLocalizedText("Average Hours Per Day", locale) + ":";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerDay;
        row++;

        return row;
    }

    private int AddDailyBreakdown(IXLWorksheet worksheet, int row, List<DailyBreakdownDto> breakdown, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Daily Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Date", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Day of Week", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Hours", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Entries Count", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var day in breakdown)
        {
            worksheet.Cell(row, 1).Value = day.Date.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 2).Value = day.DayOfWeek;
            worksheet.Cell(row, 3).Value = day.Hours;
            worksheet.Cell(row, 4).Value = day.EntriesCount;

            // Альтернативний колір рядка
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientBreakdown(IXLWorksheet worksheet, int row, List<ClientBreakdownDto> breakdown, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var client in breakdown)
        {
            worksheet.Cell(row, 1).Value = client.ClientName;
            worksheet.Cell(row, 2).Value = client.TotalHours;
            worksheet.Cell(row, 3).Value = client.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{client.Percentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddJobTypeBreakdown(IXLWorksheet worksheet, int row, List<JobTypeBreakdownDto> breakdown, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Job Type Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Job Type", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var jobType in breakdown)
        {
            worksheet.Cell(row, 1).Value = jobType.JobTypeName;
            worksheet.Cell(row, 2).Value = jobType.TotalHours;
            worksheet.Cell(row, 3).Value = jobType.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{jobType.Percentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddProjectBreakdown(IXLWorksheet worksheet, int row, List<ProjectBreakdownDto> breakdown, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Project Breakdown", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 4).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Project", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var project in breakdown)
        {
            worksheet.Cell(row, 1).Value = project.ProjectBrandName;
            worksheet.Cell(row, 2).Value = project.TotalHours;
            worksheet.Cell(row, 3).Value = project.EntriesCount;
            worksheet.Cell(row, 4).Value = $"{project.Percentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
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
            // Report titles
            "User Load Report" => isUkrainian ? HeadersUk.UserLoadReport : HeadersEn.UserLoadReport,
            "Team Load Report" => isUkrainian ? HeadersUk.TeamLoadReport : HeadersEn.TeamLoadReport,
            "Client Report" => isUkrainian ? HeadersUk.ClientReport : HeadersEn.ClientReport,
            "Time Summary Report" => isUkrainian ? HeadersUk.TimeSummaryReport : HeadersEn.TimeSummaryReport,
            "Summary Report" => isUkrainian ? "Зведений звіт" : "Summary Report",
            
            // User fields
            "User Name" => isUkrainian ? HeadersUk.UserName : HeadersEn.UserName,
            "User ID" => isUkrainian ? HeadersUk.UserId : HeadersEn.UserId,
            "Email" => "Email",
            "Agency" => isUkrainian ? HeadersUk.AgencyName : HeadersEn.AgencyName,
            
            // Date fields
            "Date" => isUkrainian ? HeadersUk.Date : HeadersEn.Date,
            "From Date" => isUkrainian ? HeadersUk.FromDate : HeadersEn.FromDate,
            "To Date" => isUkrainian ? HeadersUk.ToDate : HeadersEn.ToDate,
            "Period" => isUkrainian ? HeadersUk.Period : HeadersEn.Period,
            "Day of Week" => isUkrainian ? "День тижня" : "Day of Week",
            
            // Time fields
            "Hours" => isUkrainian ? HeadersUk.Hours : HeadersEn.Hours,
            "Total Hours" => isUkrainian ? HeadersUk.TotalHours : HeadersEn.TotalHours,
            "Average Hours" => isUkrainian ? HeadersUk.AverageHours : HeadersEn.AverageHours,
            "Average Hours Per Day" => isUkrainian ? "Середньо годин на день" : "Average Hours Per Day",
            "Working Days" => isUkrainian ? HeadersUk.WorkingDays : HeadersEn.WorkingDays,
            
            // Entities
            "Client" => isUkrainian ? HeadersUk.Client : HeadersEn.Client,
            "Project" => isUkrainian ? HeadersUk.Project : HeadersEn.Project,
            "Job Type" => isUkrainian ? HeadersUk.JobType : HeadersEn.JobType,
            "Media" => isUkrainian ? HeadersUk.Media : HeadersEn.Media,
            
            // Statistics
            "Entries Count" => isUkrainian ? HeadersUk.EntriesCount : HeadersEn.EntriesCount,
            "Percentage" => isUkrainian ? HeadersUk.Percentage : HeadersEn.Percentage,
            "Total" => isUkrainian ? HeadersUk.Total : HeadersEn.Total,
            "Statistics" => isUkrainian ? "Статистика" : "Statistics",
            
            // Sections
            "Daily Breakdown" => isUkrainian ? "Деталізація по днях" : "Daily Breakdown",
            "Client Breakdown" => isUkrainian ? "Деталізація по клієнтах" : "Client Breakdown",
            "Job Type Breakdown" => isUkrainian ? "Деталізація по типах робіт" : "Job Type Breakdown",
            "Project Breakdown" => isUkrainian ? "Деталізація по проєктах" : "Project Breakdown",
            "Members Load" => isUkrainian ? "Навантаження членів команди" : "Members Load",
            "Top Clients" => isUkrainian ? "Топ клієнти" : "Top Clients",
            "Top Users" => isUkrainian ? "Топ користувачі" : "Top Users",
            
            // Team fields
            "Total Members" => isUkrainian ? "Всього членів" : "Total Members",
            "Active Members" => isUkrainian ? "Активних членів" : "Active Members",
            "Average Hours Per Member" => isUkrainian ? "Середньо годин на члена" : "Average Hours Per Member",
            
            // Client fields
            "Unique Users" => isUkrainian ? "Унікальних користувачів" : "Unique Users",
            "Unique Projects" => isUkrainian ? "Унікальних проєктів" : "Unique Projects",
            
            // Default
            _ => key
        };
    }

    #endregion

    #region Additional Helper Methods for Team/Client/Summary Reports

    private int AddTeamStatistics(IXLWorksheet worksheet, int row, TeamLoadReportDto report, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Team Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        // Всього годин команди
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Team Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalTeamHours;
        row++;

        // Всього членів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Members", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalMembers;
        row++;

        // Активних членів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Active Members", locale) + ":";
        worksheet.Cell(row, 2).Value = report.ActiveMembers;
        row++;

        // Середньо годин на члена
        worksheet.Cell(row, 1).Value = GetLocalizedText("Average Hours Per Member", locale) + ":";
        worksheet.Cell(row, 2).Value = report.AverageHoursPerMember;
        row++;

        return row;
    }

    private int AddMembersLoad(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> members, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Members Load", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 6).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("User Name", locale);
        worksheet.Cell(row, 2).Value = "Email";
        worksheet.Cell(row, 3).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Working Days", locale);
        worksheet.Cell(row, 6).Value = GetLocalizedText("Load Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var member in members)
        {
            worksheet.Cell(row, 1).Value = member.UserName;
            worksheet.Cell(row, 2).Value = member.UserEmail;
            worksheet.Cell(row, 3).Value = member.TotalHours;
            worksheet.Cell(row, 4).Value = member.EntriesCount;
            worksheet.Cell(row, 5).Value = member.WorkingDays;
            worksheet.Cell(row, 6).Value = $"{member.LoadPercentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddTopClients(IXLWorksheet worksheet, int row, List<ClientSummaryDto> clients, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Top Clients", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("Client", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Projects Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Users Count", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var client in clients)
        {
            worksheet.Cell(row, 1).Value = client.ClientName;
            worksheet.Cell(row, 2).Value = client.TotalHours;
            worksheet.Cell(row, 3).Value = client.ProjectsCount;
            worksheet.Cell(row, 4).Value = client.UsersCount;
            worksheet.Cell(row, 5).Value = $"{client.Percentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    private int AddClientStatistics(IXLWorksheet worksheet, int row, ClientReportDto report, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        // Всього годин
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        // Всього записів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        // Унікальних користувачів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Unique Users", locale) + ":";
        worksheet.Cell(row, 2).Value = report.UniqueUsers;
        row++;

        // Унікальних проєктів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Unique Projects", locale) + ":";
        worksheet.Cell(row, 2).Value = report.UniqueProjects;
        row++;

        return row;
    }

    private int AddSummaryStatistics(IXLWorksheet worksheet, int row, TimeSummaryReportDto report, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Overall Statistics", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 2).Merge();
        row++;

        // Всього годин
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Hours", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalHours;
        row++;

        // Всього записів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Entries Count", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalEntries;
        row++;

        // Всього користувачів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Users", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalUsers;
        row++;

        // Всього клієнтів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Clients", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalClients;
        row++;

        // Всього проєктів
        worksheet.Cell(row, 1).Value = GetLocalizedText("Total Projects", locale) + ":";
        worksheet.Cell(row, 2).Value = report.TotalProjects;
        row++;

        return row;
    }

    private int AddTopUsers(IXLWorksheet worksheet, int row, List<UserLoadSummaryDto> users, string locale)
    {
        // Заголовок секції
        worksheet.Cell(row, 1).Value = GetLocalizedText("Top Users", locale);
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        worksheet.Range(row, 1, row, 5).Merge();
        row++;

        // Заголовки таблиці
        var headerRow = row;
        worksheet.Cell(row, 1).Value = GetLocalizedText("User Name", locale);
        worksheet.Cell(row, 2).Value = GetLocalizedText("Total Hours", locale);
        worksheet.Cell(row, 3).Value = GetLocalizedText("Entries Count", locale);
        worksheet.Cell(row, 4).Value = GetLocalizedText("Working Days", locale);
        worksheet.Cell(row, 5).Value = GetLocalizedText("Load Percentage", locale);

        // Стилізація заголовків
        var headerRange = worksheet.Range(row, 1, row, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(ExcelStyles.HeaderBackgroundColor);
        headerRange.Style.Font.FontColor = XLColor.FromHtml(ExcelStyles.HeaderFontColor);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        row++;

        // Дані
        foreach (var user in users)
        {
            worksheet.Cell(row, 1).Value = user.UserName;
            worksheet.Cell(row, 2).Value = user.TotalHours;
            worksheet.Cell(row, 3).Value = user.EntriesCount;
            worksheet.Cell(row, 4).Value = user.WorkingDays;
            worksheet.Cell(row, 5).Value = $"{user.LoadPercentage:F2}%";

            // Альтернативний колір
            if ((row - headerRow) % 2 == 0)
            {
                worksheet.Range(row, 1, row, 5).Style.Fill.BackgroundColor = 
                    XLColor.FromHtml(ExcelStyles.AlternateRowColor);
            }

            // Рамки
            worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        return row;
    }

    #endregion
}