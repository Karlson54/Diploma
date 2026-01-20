using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Core.Services.TimeTracking;

namespace TimeTracker.API.Controllers.TimeTracking;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
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

    [HttpGet("my")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEntries(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var entries = await _timeEntryService.GetUserEntriesAsync(
                currentUserId, 
                fromDate, 
                toDate);

            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні записів часу");
            return BadRequest(new { Message = ex.Message });
        }
    }

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
            
            // Якщо UserId не вказаний або користувач намагається створити запис для себе
            if (dto.UserId == 0 || dto.UserId == currentUserId)
            {
                dto.UserId = currentUserId;
            }

            var entry = await _timeEntryService.CreateAsync(dto, currentUserId);
            
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
            var entry = await _timeEntryService.UpdateAsync(id, dto, currentUserId);

            _logger.LogInformation(
                "TimeEntry оновлено. Id: {Id}, UserId: {UserId}",
                entry.Id, entry.UserId);

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
            _logger.LogError(ex, "Помилка при оновленні запису {Id}", id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanEditOwnTimeEntry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _timeEntryService.DeleteAsync(id, currentUserId);

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

    [HttpGet("paged")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
            
            // Якщо userId не вказаний - показуємо записи поточного користувача
            var targetUserId = userId ?? currentUserId;

            var (entries, totalCount) = await _timeEntryService.GetPagedAsync(
                pageNumber,
                pageSize,
                targetUserId,
                agencyId,
                clientId,
                fromDate,
                toDate,
                currentUserId);

            return Ok(new
            {
                Data = entries,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Filters = new
                {
                    UserId = targetUserId,
                    AgencyId = agencyId,
                    ClientId = clientId,
                    FromDate = fromDate,
                    ToDate = toDate
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні сторінки записів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBulk([FromBody] List<CreateTimeEntryDto> dtos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!dtos.Any())
            return BadRequest(new { Message = "Список записів порожній" });

        if (dtos.Count > 100)
            return BadRequest(new { Message = "Неможливо створити більше 100 записів за один раз" });

        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Встановлюємо поточного користувача для всіх записів
            foreach (var dto in dtos)
            {
                if (dto.UserId == 0)
                    dto.UserId = currentUserId;
            }

            var entries = await _timeEntryService.CreateBulkAsync(dtos, currentUserId);

            _logger.LogInformation(
                "Bulk створення завершено. Створено {Count} записів",
                entries.Count());

            return CreatedAtAction(
                nameof(GetPaged),
                new { pageNumber = 1, pageSize = 20 },
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
            _logger.LogError(ex, "Помилка при bulk створенні записів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("bulk")]
    [Authorize(Policy = "CanEditOwnTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBulk([FromBody] List<BulkUpdateTimeEntryDto> updates)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!updates.Any())
            return BadRequest(new { Message = "Список оновлень порожній" });

        if (updates.Count > 100)
            return BadRequest(new { Message = "Неможливо оновити більше 100 записів за один раз" });

        try
        {
            var currentUserId = GetCurrentUserId();
            var updateTuples = updates.Select(u => (u.Id, u.Data)).ToList();
            
            var entries = await _timeEntryService.UpdateBulkAsync(updateTuples, currentUserId);

            _logger.LogInformation(
                "Bulk оновлення завершено. Оновлено {Count} записів",
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
            _logger.LogError(ex, "Помилка при bulk оновленні записів");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("bulk")]
    [Authorize(Policy = "CanEditOwnTimeEntry")]
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
            await _timeEntryService.DeleteBulkAsync(ids, currentUserId);

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

    [HttpPost("copy-day")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CopyDay([FromBody] CopyDayDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = dto.UserId ?? currentUserId;

            var entries = await _timeEntryService.CopyDayEntriesAsync(
                targetUserId,
                dto.SourceDate,
                dto.TargetDate,
                currentUserId);

            _logger.LogInformation(
                "Копіювання дня завершено. UserId: {UserId}, Count: {Count}",
                targetUserId, entries.Count());

            return CreatedAtAction(
                nameof(GetPaged),
                new { pageNumber = 1, pageSize = 20 },
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
            _logger.LogError(ex, "Помилка при копіюванні дня");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("copy-week")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(IEnumerable<TimeEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CopyWeek([FromBody] CopyWeekDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = dto.UserId ?? currentUserId;

            var entries = await _timeEntryService.CopyWeekEntriesAsync(
                targetUserId,
                dto.SourceWeekStart,
                dto.TargetWeekStart,
                currentUserId);

            _logger.LogInformation(
                "Копіювання тижня завершено. UserId: {UserId}, Count: {Count}",
                targetUserId, entries.Count());

            return CreatedAtAction(
                nameof(GetPaged),
                new { pageNumber = 1, pageSize = 20 },
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
            _logger.LogError(ex, "Помилка при копіюванні тижня");
            return BadRequest(new { Message = ex.Message });
        }
    }
    [HttpGet("summary/day")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySummary(
        [FromQuery] DateTime date,
        [FromQuery] long? userId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = userId ?? currentUserId;

            // Якщо запитують чужу статистику - перевіряємо права
            if (targetUserId != currentUserId && 
                !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var summary = await _timeEntryService.GetDailySummaryAsync(targetUserId, date);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні денного підсумку");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("summary/week")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeeklySummary(
        [FromQuery] DateTime weekStart,
        [FromQuery] long? userId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = userId ?? currentUserId;

            if (targetUserId != currentUserId && 
                !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var summary = await _timeEntryService.GetWeeklySummaryAsync(targetUserId, weekStart);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні тижневого підсумку");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("summary/month")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] long? userId = null)
    {
        if (month < 1 || month > 12)
            return BadRequest(new { Message = "Місяць має бути від 1 до 12" });

        if (year < 2000 || year > 2100)
            return BadRequest(new { Message = "Рік має бути від 2000 до 2100" });

        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = userId ?? currentUserId;

            if (targetUserId != currentUserId && 
                !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var summary = await _timeEntryService.GetMonthlySummaryAsync(targetUserId, year, month);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні місячного підсумку");
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("remaining-hours")]
    [Authorize(Policy = "CanCreateTimeEntry")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRemainingHours(
        [FromQuery] DateTime date,
        [FromQuery] long? excludeEntryId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var remainingMs = await _timeEntryService.GetRemainingHoursForDayAsync(
                currentUserId, 
                date, 
                excludeEntryId);

            return Ok(new
            {
                Date = date.Date,
                RemainingHoursMs = remainingMs,
                RemainingHours = TimeSpan.FromMilliseconds(remainingMs).ToString(@"hh\:mm"),
                MaxDailyHoursMs = Core.Common.ValidationConstants.MaxHoursPerDayMs,
                MaxDailyHours = "24:00"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні залишку годин");
            return BadRequest(new { Message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("Не вдалося отримати userId з токена");
            throw new UnauthorizedAccessException("Невалідний токен");
        }

        return userId;
    }
}