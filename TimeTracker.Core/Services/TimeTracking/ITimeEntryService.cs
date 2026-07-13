using TimeTracker.Core.DTOs.TimeEntries;

namespace TimeTracker.Core.Services.TimeTracking;

public interface ITimeEntryService
{
    // Базовые CRUD операции
    Task<TimeEntryDetailDto?> GetByIdAsync(long id, long requestingUserId);

    Task<IEnumerable<TimeEntryListItemDto>> GetUserEntriesAsync(
        long userId,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<TimeEntryDto> CreateAsync(
        CreateTimeEntryDto dto,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent);

    Task<TimeEntryDto> UpdateAsync(
        long id,
        UpdateTimeEntryDto dto,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent);

    Task DeleteAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    // Пагинация
    Task<(IEnumerable<TimeEntryListItemDto> Entries, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null,
        long? departmentId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        long requestingUserId = 0);

    // Bulk операции
    Task<IEnumerable<TimeEntryDto>> CreateBulkAsync(
        IEnumerable<CreateTimeEntryDto> dtos,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent);

    Task<IEnumerable<TimeEntryDto>> UpdateBulkAsync(
        IEnumerable<(long Id, UpdateTimeEntryDto Dto)> updates,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task DeleteBulkAsync(
        IEnumerable<long> ids,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    // Копирование записей
    Task<IEnumerable<TimeEntryDto>> CopyDayEntriesAsync(
        long userId,
        DateTime sourceDate,
        DateTime targetDate,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task<IEnumerable<TimeEntryDto>> CopyWeekEntriesAsync(
        long userId,
        DateTime sourceWeekStart,
        DateTime targetWeekStart,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    Task<IEnumerable<TimeEntryDto>> CopyEntriesByIdsAsync(
        IEnumerable<long> entryIds,
        DateTime targetDate,
        long requestingUserId,
        string ipAddress,
        string userAgent);

    // Статистика и аналитика
    Task<object> GetDailySummaryAsync(long userId, DateTime date);
    Task<object> GetWeeklySummaryAsync(long userId, DateTime weekStart);
    Task<object> GetMonthlySummaryAsync(long userId, int year, int month);

    // Валидация
    Task<bool> CanUserEditEntryAsync(long entryId, long requestingUserId);
    Task<long> GetRemainingHoursForDayAsync(long userId, DateTime date, long? excludeEntryId = null);
}