// TimeTracker.Data/Repositories/TimeEntries/ITimeEntryRepository.cs
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.TimeEntries;

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    // Получение записей времени
    Task<IEnumerable<TimeEntry>> GetUserTimeEntriesAsync(long userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<TimeEntry?> GetUserTimeEntryAsync(long userId, long entryId);
    
    // Валидация и проверки
    Task<bool> HasTimeEntryForDateAsync(long userId, DateTime date);
    Task<long> GetTotalHoursForDateAsync(long userId, DateTime date);
    Task<bool> CanAddTimeAsync(long userId, DateTime date, long hoursMilliseconds);
    
    // Отчеты и аналитика
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByPeriodAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByAgencyAsync(long agencyId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByClientAsync(long clientId, DateTime fromDate, DateTime toDate);
    
    // Статистика
    Task<Dictionary<long, long>> GetUserTotalHoursAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<long, long>> GetClientTotalHoursAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<long, long>> GetProjectTotalHoursAsync(DateTime fromDate, DateTime toDate);
    
    // Фильтрация с пагинацией
    Task<(IEnumerable<TimeEntry> Entries, int TotalCount)> GetTimeEntriesPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
}