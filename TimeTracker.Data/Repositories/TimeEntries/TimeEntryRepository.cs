using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Context;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.TimeEntries;

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    private const long MaxHoursPerDayMs = 86400000;

    public TimeEntryRepository(TimeTrackerDbContext context) : base(context)
    {
    }

    #region Получение записей времени

    public async Task<IEnumerable<TimeEntry>> GetUserTimeEntriesAsync(long userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(te => te.UserId == userId);

        if (fromDate.HasValue)
        {
            query = query.Where(te => te.EntryDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(te => te.EntryDate <= toDate.Value.Date);
        }

        return await query
            .OrderByDescending(te => te.EntryDate)
            .ThenByDescending(te => te.CreatedAt)
            .ToListAsync();
    }

    public async Task<TimeEntry?> GetUserTimeEntryAsync(long userId, long entryId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(te => te.Id == entryId && te.UserId == userId);
    }

    #endregion

    #region Валидация и проверки

    public async Task<bool> HasTimeEntryForDateAsync(long userId, DateTime date)
    {
        return await _dbSet
            .AnyAsync(te => te.UserId == userId && te.EntryDate.Date == date.Date);
    }

    public async Task<long> GetTotalHoursForDateAsync(long userId, DateTime date)
    {
        return await _dbSet
            .Where(te => te.UserId == userId && te.EntryDate.Date == date.Date)
            .SumAsync(te => te.HoursMilliseconds);
    }

    public async Task<bool> CanAddTimeAsync(long userId, DateTime date, long hoursMilliseconds)
    {
        var totalExistingHours = await GetTotalHoursForDateAsync(userId, date);
        return (totalExistingHours + hoursMilliseconds) <= MaxHoursPerDayMs;
    }

    #endregion

    #region Отчеты и аналитика

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByPeriodAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .OrderBy(te => te.EntryDate)
            .ThenBy(te => te.UserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByAgencyAsync(long agencyId, DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => 
                te.AgencyId == agencyId &&
                te.EntryDate >= fromDate.Date && 
                te.EntryDate <= toDate.Date)
            .OrderBy(te => te.EntryDate)
            .ThenBy(te => te.UserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByClientAsync(long clientId, DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => 
                te.ClientId == clientId &&
                te.EntryDate >= fromDate.Date && 
                te.EntryDate <= toDate.Date)
            .OrderBy(te => te.EntryDate)
            .ThenBy(te => te.UserId)
            .ToListAsync();
    }

    #endregion

    #region Статистика

    public async Task<Dictionary<long, long>> GetUserTotalHoursAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .GroupBy(te => te.UserId)
            .Select(g => new { UserId = g.Key, TotalHours = g.Sum(te => te.HoursMilliseconds) })
            .ToDictionaryAsync(x => x.UserId, x => x.TotalHours);
    }

    public async Task<Dictionary<long, long>> GetClientTotalHoursAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .GroupBy(te => te.ClientId)
            .Select(g => new { ClientId = g.Key, TotalHours = g.Sum(te => te.HoursMilliseconds) })
            .ToDictionaryAsync(x => x.ClientId, x => x.TotalHours);
    }

    public async Task<Dictionary<long, long>> GetProjectTotalHoursAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .GroupBy(te => te.ProjectBrandId)
            .Select(g => new { ProjectId = g.Key, TotalHours = g.Sum(te => te.HoursMilliseconds) })
            .ToDictionaryAsync(x => x.ProjectId, x => x.TotalHours);
    }

    #endregion

    #region Фильтрация с пагинацией

    public async Task<(IEnumerable<TimeEntry> Entries, int TotalCount)> GetTimeEntriesPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        // Применяем фильтры
        if (userId.HasValue)
        {
            query = query.Where(te => te.UserId == userId.Value);
        }

        if (agencyId.HasValue)
        {
            query = query.Where(te => te.AgencyId == agencyId.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(te => te.ClientId == clientId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(te => te.EntryDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(te => te.EntryDate <= toDate.Value.Date);
        }

        var totalCount = await query.CountAsync();

        var entries = await query
            .OrderByDescending(te => te.EntryDate)
            .ThenByDescending(te => te.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (entries, totalCount);
    }

    #endregion

    #region Дополнительные методы для отчетности

    /// <summary>
    /// Получить записи времени с загруженными связанными данными для отчетов
    /// </summary>
    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesWithDetailsAsync(
        DateTime fromDate, 
        DateTime toDate,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date);

        if (userId.HasValue)
        {
            query = query.Where(te => te.UserId == userId.Value);
        }

        if (agencyId.HasValue)
        {
            query = query.Where(te => te.AgencyId == agencyId.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(te => te.ClientId == clientId.Value);
        }

        return await query
            .OrderBy(te => te.EntryDate)
            .ThenBy(te => te.User.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Получить сводную статистику по пользователям за период
    /// </summary>
    public async Task<IEnumerable<object>> GetUserSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .GroupBy(te => new { te.UserId, te.User.Name })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                UserName = g.Key.Name,
                TotalHours = g.Sum(te => te.HoursMilliseconds),
                TotalEntries = g.Count(),
                AverageHoursPerDay = g.Sum(te => te.HoursMilliseconds) / (double)g.Select(te => te.EntryDate).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalHours)
            .ToListAsync();
    }

    /// <summary>
    /// Получить статистику по клиентам за период
    /// </summary>
    public async Task<IEnumerable<object>> GetClientSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .GroupBy(te => new { te.ClientId, te.Client.Name })
            .Select(g => new
            {
                ClientId = g.Key.ClientId,
                ClientName = g.Key.Name,
                TotalHours = g.Sum(te => te.HoursMilliseconds),
                TotalEntries = g.Count(),
                UniqueUsers = g.Select(te => te.UserId).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalHours)
            .ToListAsync();
    }

    /// <summary>
    /// Валидация времени при обновлении существующей записи
    /// </summary>
    public async Task<bool> CanUpdateTimeAsync(long entryId, long userId, DateTime date, long newHoursMilliseconds)
    {
        // Получаем текущее количество часов без учета обновляемой записи
        var totalExistingHours = await _dbSet
            .Where(te => te.UserId == userId && 
                        te.EntryDate.Date == date.Date && 
                        te.Id != entryId)
            .SumAsync(te => te.HoursMilliseconds);

        return (totalExistingHours + newHoursMilliseconds) <= MaxHoursPerDayMs;
    }

    /// <summary>
    /// Получить дневную статистику пользователя
    /// </summary>
    public async Task<object?> GetDailySummaryAsync(long userId, DateTime date)
    {
        var entries = await _dbSet
            .AsNoTracking()
            .Where(te => te.UserId == userId && te.EntryDate.Date == date.Date)
            .ToListAsync();

        if (!entries.Any())
            return null;

        return new
        {
            Date = date.Date,
            TotalHours = entries.Sum(te => te.HoursMilliseconds),
            TotalEntries = entries.Count,
            Entries = entries.Select(te => new
            {
                te.Id,
                te.HoursMilliseconds,
                te.Comments,
                te.CreatedAt
            })
        };
    }

    #endregion
}