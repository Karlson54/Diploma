using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Services.Reporting;
using TimeTracker.Core.DTOs.Reports;

namespace TimeTracker.API.Controllers.Reports;

/// <summary>
/// Генерація звітів та експорт даних
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IExportService _exportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        IExportService exportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Звіт завантаженості конкретного співробітника
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <param name="fromDate">Початок діапазону</param>
    /// <param name="toDate">Кінець діапазону</param>
    [HttpGet("user/{userId}")]
    [Authorize(Policy = "CanViewOwnReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLoadReport(
        long userId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetUserLoadReportAsync(
                userId, fromDate, toDate, currentUserId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації звіту користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Експортувати звіт співробітника у Excel
    /// </summary>
    [HttpGet("user/{userId}/export/excel")]
    [Authorize(Policy = "CanViewOwnReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUserLoadReportExcel(
        long userId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportUserLoadReportToExcelAsync(
                userId, fromDate, toDate, currentUserId, ipAddress, userAgent);

            var fileName = $"UserLoadReport_{userId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Експорт записів співробітника як плоска таблиця з вибором колонок
    /// </summary>
    [HttpGet("user/{userId}/export/flat")]
    [Authorize(Policy = "CanViewOwnReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUserEntriesFlat(
        long userId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? columns = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var columnsDto = ExportColumnsDto.FromString(columns);

            var fileBytes = await _exportService.ExportUserEntriesFlatToExcelAsync(
                userId, fromDate, toDate, currentUserId, ipAddress, userAgent, columnsDto);

            var fileName = $"Report_{userId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при flat-експорті записів користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Експорт всіх записів як плоска таблиця з вибором колонок (тільки Admin)
    /// </summary>
    [HttpGet("export/flat")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAllEntriesFlat(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? columns = null,
        [FromQuery] long? agencyId = null,
        [FromQuery] long? departmentId = null,
        [FromQuery] string? userIds = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var columnsDto = ExportColumnsDto.FromString(columns);

            IEnumerable<long>? userIdsList = null;
            if (!string.IsNullOrWhiteSpace(userIds))
            {
                userIdsList = userIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(id => long.TryParse(id, out var parsed) ? parsed : 0)
                    .Where(id => id > 0);
            }

            var fileBytes = await _exportService.ExportAllEntriesFlatToExcelAsync(
                fromDate, toDate, currentUserId, ipAddress, userAgent, columnsDto,
                agencyId, departmentId, userIdsList);

            var fileName = $"AllReports_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при flat-експорті всіх записів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Звіт завантаженості команди агентства
    /// </summary>
    /// <param name="agencyId">Ідентифікатор агентства</param>
    [HttpGet("team/{agencyId}")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamLoadReport(
        long agencyId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetTeamLoadReportAsync(
                agencyId, fromDate, toDate, currentUserId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації звіту команди {AgencyId}", agencyId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Експортувати звіт команди у Excel
    /// </summary>
    [HttpGet("team/{agencyId}/export/excel")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTeamLoadReportExcel(
        long agencyId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTeamLoadReportToExcelAsync(
                agencyId, fromDate, toDate, currentUserId, ipAddress, userAgent);

            var fileName = $"TeamLoadReport_{agencyId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту команди {AgencyId}", agencyId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Звіт по клієнту (тільки Manager/Admin/Accountant)
    /// </summary>
    /// <param name="clientId">Ідентифікатор клієнта</param>
    [HttpGet("client/{clientId}")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientReport(
        long clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetClientReportAsync(
                clientId, fromDate, toDate, currentUserId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації звіту клієнта {ClientId}", clientId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Експортувати звіт по клієнту у Excel
    /// </summary>
    [HttpGet("client/{clientId}/export/excel")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportClientReportExcel(
        long clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportClientReportToExcelAsync(
                clientId, fromDate, toDate, currentUserId, ipAddress, userAgent);

            var fileName = $"ClientReport_{clientId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту клієнта {ClientId}", clientId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Топ клієнтів за витраченим часом
    /// </summary>
    /// <param name="fromDate">Початок діапазону</param>
    /// <param name="toDate">Кінець діапазону</param>
    /// <param name="agencyId">Фільтр за агентством (опціонально)</param>
    /// <param name="top">Кількість клієнтів у топі (за замовчуванням 10)</param>
    [HttpGet("clients/top")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopClients(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] long? agencyId = null,
        [FromQuery] int top = 10)
    {
        try
        {
            if (top < 1 || top > 100)
            {
                return BadRequest(new { Message = "Top має бути від 1 до 100" });
            }

            var currentUserId = GetCurrentUserId();
            var clients = await _reportService.GetTopClientsReportAsync(
                fromDate, toDate, currentUserId, agencyId, top);

            return Ok(clients);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації топ клієнтів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Активні користувачі без записів на поточному тижні
    /// </summary>
    [HttpGet("inactive-users")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInactiveUsersThisWeek()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var users = await _reportService.GetInactiveUsersThisWeekAsync(currentUserId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні неактивних користувачів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("project")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectReport(
        [FromQuery] string projectBrandName,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetProjectReportAsync(
                projectBrandName, fromDate, toDate, currentUserId);
            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації звіту проєкту {ProjectBrandName}", projectBrandName);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("summary")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTimeSummaryReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] long? agencyId = null,
        [FromQuery] long? clientId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetTimeSummaryReportAsync(
                fromDate, toDate, currentUserId, agencyId, clientId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації зведеного звіту");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("summary/export/excel")]
    [Authorize(Policy = "CanExportData")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTimeSummaryReportExcel(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] long? agencyId = null,
        [FromQuery] long? clientId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTimeSummaryReportToExcelAsync(
                fromDate, toDate, currentUserId, ipAddress, userAgent, agencyId, clientId);

            var fileName = $"TimeSummary_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті зведеного звіту");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // ==================== HELPER METHODS ====================

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Невалідний токен: не вдалося отримати userId");
            throw new UnauthorizedAccessException("Невалідний токен");
        }

        return userId;
    }

    private string GetIpAddress()
    {
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        return userAgent.Length > 500
            ? userAgent.Substring(0, 500)
            : userAgent;
    }
}