using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.Services.Reporting;

namespace TimeTracker.API.Controllers.Reports;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
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
            return Forbid(ex.Message);
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

    [HttpGet("user/{userId}/export/excel")]
    [Authorize(Policy = "CanViewOwnReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUserLoadReportExcel(
        long userId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportUserLoadReportToExcelAsync(
                userId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"UserLoadReport_{userId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("user/{userId}/export/csv")]
    [Authorize(Policy = "CanViewOwnReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUserLoadReportCsv(
        long userId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportUserLoadReportToCsvAsync(
                userId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"UserLoadReport_{userId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

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
            return Forbid(ex.Message);
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

    [HttpGet("team/{agencyId}/export/excel")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTeamLoadReportExcel(
        long agencyId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTeamLoadReportToExcelAsync(
                agencyId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"TeamLoadReport_{agencyId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту команди {AgencyId}", agencyId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("team/{agencyId}/export/csv")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTeamLoadReportCsv(
        long agencyId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTeamLoadReportToCsvAsync(
                agencyId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"TeamLoadReport_{agencyId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту команди {AgencyId}", agencyId);
            return BadRequest(new { Message = ex.Message });
        }
    }

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
            return Forbid(ex.Message);
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

    [HttpGet("client/{clientId}/export/excel")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportClientReportExcel(
        long clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportClientReportToExcelAsync(
                clientId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"ClientReport_{clientId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту клієнта {ClientId}", clientId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("client/{clientId}/export/csv")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportClientReportCsv(
        long clientId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportClientReportToCsvAsync(
                clientId, fromDate, toDate, currentUserId, ipAddress, userAgent, locale);

            var fileName = $"ClientReport_{clientId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті звіту клієнта {ClientId}", clientId);
            return BadRequest(new { Message = ex.Message });
        }
    }

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
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації топ клієнтів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("project/{projectBrandId}")]
    [Authorize(Policy = "CanViewAllReports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectReport(
        long projectBrandId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var report = await _reportService.GetProjectReportAsync(
                projectBrandId, fromDate, toDate, currentUserId);

            return Ok(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при генерації звіту проєкту {ProjectBrandId}", projectBrandId);
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
            return Forbid(ex.Message);
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
        [FromQuery] long? clientId = null,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTimeSummaryReportToExcelAsync(
                fromDate, toDate, currentUserId, ipAddress, userAgent, agencyId, clientId, locale);

            var fileName = $"TimeSummary_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті зведеного звіту");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("summary/export/csv")]
    [Authorize(Policy = "CanExportData")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTimeSummaryReportCsv(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] long? agencyId = null,
        [FromQuery] long? clientId = null,
        [FromQuery] string locale = "uk")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var fileBytes = await _exportService.ExportTimeSummaryReportToCsvAsync(
                fromDate, toDate, currentUserId, ipAddress, userAgent, agencyId, clientId, locale);

            var fileName = $"TimeSummary_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при експорті зведеного звіту CSV");
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