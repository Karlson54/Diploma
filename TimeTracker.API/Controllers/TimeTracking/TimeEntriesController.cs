using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Core.Services.TimeTracking;

namespace TimeTracker.API.Controllers.TimeTracking;

/// <summary>
/// Управління записами робочого часу
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Time Entries")]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ILogger<TimeEntriesController> _logger;

    public TimeEntriesController(
        ITimeEntryService timeEntryService,
        ILogger<TimeEntriesController> logger)
    {
        _timeEntryService = timeEntryService;
        _logger = logger;
    }

    /// <summary>
    /// Отримати запис часу за ID
    /// </summary>
    /// <param name="id">Ідентифікатор запису</param>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(TimeEntryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var entry = await _timeEntryService.GetByIdAsync(id, currentUserId);

            if (entry == null)
                return NotFound(new { Message = $"Запис часу з ID {id} не знайдено" });

            return Ok(entry);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні запису часу {Id}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати власні записи часу поточного користувача з пагінацією
    /// </summary>
    /// <param name="fromDate">Початок діапазону дат</param>
    /// <param name="toDate">Кінець діапазону дат</param>
    [HttpGet("my")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEntries(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var (entries, totalCount) = await _timeEntryService.GetPagedAsync(
                pageNumber,
                pageSize,
                userId: currentUserId,
                agencyId: null,
                clientId: null,
                fromDate,
                toDate,
                requestingUserId: currentUserId);

            return Ok(new
            {
                entries,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні записів часу");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати записи часу конкретного користувача (тільки Manager/Admin)
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <param name="fromDate">Початок діапазону дат</param>
    /// <param name="toDate">Кінець діапазону дат</param>
    [HttpGet("user/{userId}")]
    [Authorize(Policy = "CanEditAnyTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserEntries(
        long userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var entries = await _timeEntryService.GetUserEntriesAsync(
                userId,
                fromDate,
                toDate);

            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні записів користувача {UserId}", userId);
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Отримати всі записи часу з пагінацією та фільтрацією (тільки Manager/Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanEditAnyTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? userId = null,
        [FromQuery] long? agencyId = null,
        [FromQuery] long? clientId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var (entries, totalCount) = await _timeEntryService.GetPagedAsync(
                pageNumber,
                pageSize,
                userId,
                agencyId,
                clientId,
                fromDate,
                toDate,
                currentUserId);

            return Ok(new
            {
                entries,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні записів часу з пагінацією");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Створити новий запис часу
    /// </summary>
    /// <param name="dto">Дані запису часу</param>
    [HttpPost]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateTimeEntryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            // Якщо UserId не вказаний або користувач намагається створити запис для себе
            if (dto.UserId == 0 || dto.UserId == currentUserId)
            {
                dto.UserId = currentUserId;
            }

            var entry = await _timeEntryService.CreateAsync(
                dto,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntry створено успішно. Id: {Id}, UserId: {UserId}, Date: {Date}",
                entry.Id, entry.UserId, entry.EntryDate);

            return CreatedAtAction(
                nameof(GetById),
                new { id = entry.Id },
                entry);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні запису часу");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<CreateTimeEntryDto> dtos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var entries = await _timeEntryService.CreateBulkAsync(
                dtos,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntries створено масово. Кількість: {Count}",
                entries.Count());

            return CreatedAtAction(
                nameof(GetMyEntries),
                entries);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при масовому створенні записів часу");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("copy-day")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CopyDay(
        [FromQuery] long userId,
        [FromQuery] DateTime sourceDate,
        [FromQuery] DateTime targetDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var entries = await _timeEntryService.CopyDayEntriesAsync(
                userId,
                sourceDate,
                targetDate,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntries скопійовано за день. UserId: {UserId}, From: {SourceDate}, To: {TargetDate}, Count: {Count}",
                userId, sourceDate, targetDate, entries.Count());

            return CreatedAtAction(
                nameof(GetUserEntries),
                new { userId },
                entries);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при копіюванні записів за день");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("copy-week")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CopyWeek(
        [FromQuery] long userId,
        [FromQuery] DateTime sourceWeekStart,
        [FromQuery] DateTime targetWeekStart)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var entries = await _timeEntryService.CopyWeekEntriesAsync(
                userId,
                sourceWeekStart,
                targetWeekStart,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntries скопійовано за тиждень. UserId: {UserId}, From: {SourceWeekStart}, To: {TargetWeekStart}, Count: {Count}",
                userId, sourceWeekStart, targetWeekStart, entries.Count());

            return CreatedAtAction(
                nameof(GetUserEntries),
                new { userId },
                entries);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при копіюванні записів за тиждень");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Оновити запис часу
    /// </summary>
    /// <param name="id">Ідентифікатор запису</param>
    /// <param name="dto">Нові дані запису</param>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditOwnTimeEntry")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTimeEntryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var entry = await _timeEntryService.UpdateAsync(
                id,
                dto,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntry оновлено. Id: {Id}, UserId: {UserId}",
                id, entry.UserId);

            return Ok(entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні запису часу {Id}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("bulk")]
    [Authorize(Policy = "CanEditOwnTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBulk([FromBody] IEnumerable<(long Id, UpdateTimeEntryDto Dto)> updates)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var entries = await _timeEntryService.UpdateBulkAsync(
                updates,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "TimeEntries оновлено масово. Кількість: {Count}",
                entries.Count());

            return Ok(entries);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при масовому оновленні записів часу");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Видалити запис часу
    /// </summary>
    /// <param name="id">Ідентифікатор запису</param>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteOwnTimeEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            await _timeEntryService.DeleteAsync(
                id,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation("TimeEntry видалено. Id: {Id}", id);

            return Ok(new { Message = "Запис часу успішно видалено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні запису {Id}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("bulk")]
    [Authorize(Policy = "CanDeleteOwnTimeEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBulk([FromBody] List<long> ids)
    {
        if (!ids.Any())
            return BadRequest(new { Message = "Список ID порожній" });

        if (ids.Count > 100)
            return BadRequest(new { Message = "Неможливо видалити більше 100 записів за один раз" });

        try
        {
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            await _timeEntryService.DeleteBulkAsync(
                ids,
                currentUserId,
                ipAddress,
                userAgent);

            _logger.LogInformation(
                "Bulk видалення завершено. Видалено {Count} записів",
                ids.Count);

            return Ok(new { Message = $"Успішно видалено {ids.Count} записів" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при bulk видаленні записів");
            return BadRequest(new { Message = ex.Message });
        }
    }

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
        // Перевіряємо X-Forwarded-For (якщо за proxy/load balancer)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Перевіряємо X-Real-IP
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        // Використовуємо RemoteIpAddress
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Обмежуємо розмір (БД constraint 500 символів)
        return userAgent.Length > 500
            ? userAgent.Substring(0, 500)
            : userAgent;
    }
}