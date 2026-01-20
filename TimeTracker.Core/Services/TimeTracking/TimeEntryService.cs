using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.TimeTracking;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITimeValidationService _validationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TimeEntryService> _logger;

    public TimeEntryService(
        ITimeEntryRepository timeEntryRepository,
        IUserRepository userRepository,
        ITimeValidationService validationService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TimeEntryService> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _userRepository = userRepository;
        _validationService = validationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TimeEntryDetailDto?> GetByIdAsync(long id, long requestingUserId)
    {
        var entry = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (entry == null)
            return null;

        // Проверяем права доступа
        if (!await CanUserViewEntryAsync(entry, requestingUserId))
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався отримати доступ до запису {EntryId}, який не належить йому",
                requestingUserId, id);
            throw new UnauthorizedAccessException("Ви не маєте доступу до цього запису");
        }

        return _mapper.Map<TimeEntryDetailDto>(entry);
    }

    public async Task<IEnumerable<TimeEntryListItemDto>> GetUserEntriesAsync(
        long userId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var entries = await _timeEntryRepository.GetUserTimeEntriesAsync(userId, fromDate, toDate);

        var entriesWithDetails = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Where(te => entries.Select(e => e.Id).Contains(te.Id))
            .OrderByDescending(te => te.EntryDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<TimeEntryListItemDto>>(entriesWithDetails);
    }

    public async Task<TimeEntryDto> CreateAsync(CreateTimeEntryDto dto, long requestingUserId)
    {
        // 1. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            dto.UserId);

        if (!userPermissionResult.IsValid)
        {
            throw new UnauthorizedAccessException(
                string.Join("; ", userPermissionResult.Errors));
        }

        // 2. Валідація даних запису
        var createValidationResult = await _validationService.ValidateCreateAsync(
            dto.UserId,
            dto.EntryDate,
            dto.HoursMilliseconds);

        if (!createValidationResult.IsValid)
        {
            throw new InvalidOperationException(
                string.Join("; ", createValidationResult.Errors));
        }

        // 3. Валідація зовнішніх ключів
        var referencesValidationResult = await _validationService.ValidateReferencesAsync(
            dto.AgencyId,
            dto.MarketId,
            dto.ContractingAgencyId,
            dto.ClientId,
            dto.ProjectBrandId,
            dto.MediaId,
            dto.JobTypeId);

        if (!referencesValidationResult.IsValid)
        {
            throw new InvalidOperationException(
                string.Join("; ", referencesValidationResult.Errors));
        }

        // 4. Створення запису
        var timeEntry = _mapper.Map<TimeEntry>(dto);

        await _timeEntryRepository.AddAsync(timeEntry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "TimeEntry створено. Id: {Id}, UserId: {UserId}, Date: {Date}, Hours: {Hours}",
            timeEntry.Id, timeEntry.UserId, timeEntry.EntryDate,
            TimeHelper.FormatHours(timeEntry.HoursMilliseconds));

        // 5. Завантажуємо створений запис з усіма зв'язками
        var createdEntry = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .FirstOrDefaultAsync(te => te.Id == timeEntry.Id);

        return _mapper.Map<TimeEntryDto>(createdEntry!);
    }

    public async Task<TimeEntryDto> UpdateAsync(long id, UpdateTimeEntryDto dto, long requestingUserId)
    {
        // 1. Отримуємо існуючий запис
        var existingEntry = await _timeEntryRepository.GetByIdAsync(id);
        if (existingEntry == null)
        {
            throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
        }

        // 2. Перевіряємо права доступу
        if (!await CanUserEditEntryAsync(id, requestingUserId))
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався редагувати запис {EntryId}, який йому не належить",
                requestingUserId, id);
            throw new UnauthorizedAccessException("Ви не маєте прав редагувати цей запис");
        }

        // 3. Валідація оновлення
        var updateValidationResult = await _validationService.ValidateUpdateAsync(
            id,
            existingEntry.UserId,
            dto.EntryDate,
            dto.HoursMilliseconds);

        if (!updateValidationResult.IsValid)
        {
            throw new InvalidOperationException(
                string.Join("; ", updateValidationResult.Errors));
        }

        // 4. Валідація зовнішніх ключів
        var referencesValidationResult = await _validationService.ValidateReferencesAsync(
            dto.AgencyId,
            dto.MarketId,
            dto.ContractingAgencyId,
            dto.ClientId,
            dto.ProjectBrandId,
            dto.MediaId,
            dto.JobTypeId);

        if (!referencesValidationResult.IsValid)
        {
            throw new InvalidOperationException(
                string.Join("; ", referencesValidationResult.Errors));
        }

        // 5. Оновлюємо дані
        var oldDate = existingEntry.EntryDate;
        var oldHours = existingEntry.HoursMilliseconds;

        _mapper.Map(dto, existingEntry);

        _timeEntryRepository.Update(existingEntry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "TimeEntry оновлено. Id: {Id}, UserId: {UserId}, " +
            "OldDate: {OldDate} -> NewDate: {NewDate}, " +
            "OldHours: {OldHours} -> NewHours: {NewHours}",
            id, existingEntry.UserId,
            oldDate, existingEntry.EntryDate,
            TimeHelper.FormatHours(oldHours), TimeHelper.FormatHours(existingEntry.HoursMilliseconds));

        // 6. Завантажуємо оновлений запис
        var updatedEntry = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .FirstOrDefaultAsync(te => te.Id == id);

        return _mapper.Map<TimeEntryDto>(updatedEntry!);
    }

    public async Task DeleteAsync(long id, long requestingUserId)
    {
        var entry = await _timeEntryRepository.GetByIdAsync(id);
        if (entry == null)
        {
            throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
        }

        if (!await CanUserEditEntryAsync(id, requestingUserId))
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався видалити запис {EntryId}, який йому не належить",
                requestingUserId, id);
            throw new UnauthorizedAccessException("Ви не маєте прав видалити цей запис");
        }

        _timeEntryRepository.Delete(entry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "TimeEntry видалено. Id: {Id}, UserId: {UserId}, Date: {Date}",
            id, entry.UserId, entry.EntryDate);
    }

    public async Task<(IEnumerable<TimeEntryListItemDto> Entries, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        long requestingUserId = 0)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Якщо запитують записи іншого користувача - перевіряємо права
        if (userId.HasValue && userId.Value != requestingUserId && requestingUserId > 0)
        {
            var hasPermission = await HasManagerOrAdminRoleAsync(requestingUserId);
            if (!hasPermission)
            {
                throw new UnauthorizedAccessException(
                    "Ви не маєте прав переглядати записи інших користувачів");
            }
        }

        var (entries, totalCount) = await _timeEntryRepository.GetTimeEntriesPagedAsync(
            pageNumber,
            pageSize,
            userId,
            agencyId,
            clientId,
            fromDate,
            toDate);

        var entriesWithDetails = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Where(te => entries.Select(e => e.Id).Contains(te.Id))
            .OrderByDescending(te => te.EntryDate)
            .ThenByDescending(te => te.CreatedAt)
            .ToListAsync();

        var dtos = _mapper.Map<IEnumerable<TimeEntryListItemDto>>(entriesWithDetails);

        return (dtos, totalCount);
    }

    public async Task<IEnumerable<TimeEntryDto>> CreateBulkAsync(
        IEnumerable<CreateTimeEntryDto> dtos,
        long requestingUserId)
    {
        var dtosList = dtos.ToList();

        if (!dtosList.Any())
        {
            return Enumerable.Empty<TimeEntryDto>();
        }

        if (dtosList.Count > 100)
        {
            throw new InvalidOperationException(
                "Неможливо створити більше 100 записів за один раз");
        }

        var createdEntries = new List<TimeEntry>();

        try
        {
            foreach (var dto in dtosList)
            {
                // Валідація кожного запису
                var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
                    requestingUserId,
                    dto.UserId);

                if (!userPermissionResult.IsValid)
                {
                    throw new UnauthorizedAccessException(
                        $"Помилка валідації для запису UserId={dto.UserId}: " +
                        string.Join("; ", userPermissionResult.Errors));
                }

                var createValidationResult = await _validationService.ValidateCreateAsync(
                    dto.UserId,
                    dto.EntryDate,
                    dto.HoursMilliseconds);

                if (!createValidationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Помилка валідації для запису Date={dto.EntryDate:yyyy-MM-dd}: " +
                        string.Join("; ", createValidationResult.Errors));
                }

                var referencesValidationResult = await _validationService.ValidateReferencesAsync(
                    dto.AgencyId,
                    dto.MarketId,
                    dto.ContractingAgencyId,
                    dto.ClientId,
                    dto.ProjectBrandId,
                    dto.MediaId,
                    dto.JobTypeId);

                if (!referencesValidationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Помилка валідації довідників для запису Date={dto.EntryDate:yyyy-MM-dd}: " +
                        string.Join("; ", referencesValidationResult.Errors));
                }

                var timeEntry = _mapper.Map<TimeEntry>(dto);
                await _timeEntryRepository.AddAsync(timeEntry);
                createdEntries.Add(timeEntry);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk створення завершено. Створено {Count} записів користувачем {RequestingUserId}",
                createdEntries.Count, requestingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при bulk створенні записів користувачем {RequestingUserId}",
                requestingUserId);
            throw;
        }

        // Завантажуємо створені записи з усіма зв'язками
        var createdIds = createdEntries.Select(e => e.Id).ToList();
        var entriesWithDetails = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Where(te => createdIds.Contains(te.Id))
            .ToListAsync();

        return _mapper.Map<IEnumerable<TimeEntryDto>>(entriesWithDetails);
    }

    public async Task<IEnumerable<TimeEntryDto>> UpdateBulkAsync(
        IEnumerable<(long Id, UpdateTimeEntryDto Dto)> updates,
        long requestingUserId)
    {
        var updatesList = updates.ToList();

        if (!updatesList.Any())
        {
            return Enumerable.Empty<TimeEntryDto>();
        }

        if (updatesList.Count > 100)
        {
            throw new InvalidOperationException(
                "Неможливо оновити більше 100 записів за один раз");
        }

        var updatedEntries = new List<TimeEntry>();

        try
        {
            foreach (var (id, dto) in updatesList)
            {
                var existingEntry = await _timeEntryRepository.GetByIdAsync(id);
                if (existingEntry == null)
                {
                    throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
                }

                if (!await CanUserEditEntryAsync(id, requestingUserId))
                {
                    throw new UnauthorizedAccessException(
                        $"Ви не маєте прав редагувати запис ID={id}");
                }

                var updateValidationResult = await _validationService.ValidateUpdateAsync(
                    id,
                    existingEntry.UserId,
                    dto.EntryDate,
                    dto.HoursMilliseconds);

                if (!updateValidationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Помилка валідації для запису ID={id}: " +
                        string.Join("; ", updateValidationResult.Errors));
                }

                var referencesValidationResult = await _validationService.ValidateReferencesAsync(
                    dto.AgencyId,
                    dto.MarketId,
                    dto.ContractingAgencyId,
                    dto.ClientId,
                    dto.ProjectBrandId,
                    dto.MediaId,
                    dto.JobTypeId);

                if (!referencesValidationResult.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Помилка валідації довідників для запису ID={id}: " +
                        string.Join("; ", referencesValidationResult.Errors));
                }

                _mapper.Map(dto, existingEntry);
                _timeEntryRepository.Update(existingEntry);
                updatedEntries.Add(existingEntry);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk оновлення завершено. Оновлено {Count} записів користувачем {RequestingUserId}",
                updatedEntries.Count, requestingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при bulk оновленні записів користувачем {RequestingUserId}",
                requestingUserId);
            throw;
        }

        // Завантажуємо оновлені записи
        var updatedIds = updatedEntries.Select(e => e.Id).ToList();
        var entriesWithDetails = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Where(te => updatedIds.Contains(te.Id))
            .ToListAsync();

        return _mapper.Map<IEnumerable<TimeEntryDto>>(entriesWithDetails);
    }

    public async Task DeleteBulkAsync(IEnumerable<long> ids, long requestingUserId)
    {
        var idsList = ids.ToList();

        if (!idsList.Any())
        {
            return;
        }

        if (idsList.Count > 100)
        {
            throw new InvalidOperationException(
                "Неможливо видалити більше 100 записів за один раз");
        }

        try
        {
            var entriesToDelete = new List<TimeEntry>();

            foreach (var id in idsList)
            {
                var entry = await _timeEntryRepository.GetByIdAsync(id);
                if (entry == null)
                {
                    throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
                }

                if (!await CanUserEditEntryAsync(id, requestingUserId))
                {
                    throw new UnauthorizedAccessException(
                        $"Ви не маєте прав видалити запис ID={id}");
                }

                entriesToDelete.Add(entry);
            }

            _timeEntryRepository.DeleteRange(entriesToDelete);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Bulk видалення завершено. Видалено {Count} записів користувачем {RequestingUserId}",
                entriesToDelete.Count, requestingUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при bulk видаленні записів користувачем {RequestingUserId}",
                requestingUserId);
            throw;
        }
    }

    public async Task<IEnumerable<TimeEntryDto>> CopyDayEntriesAsync(
        long userId,
        DateTime sourceDate,
        DateTime targetDate,
        long requestingUserId)
    {
        // Перевірка прав
        var permissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            userId);

        if (!permissionResult.IsValid)
        {
            throw new UnauthorizedAccessException(
                string.Join("; ", permissionResult.Errors));
        }

        // Перевірка що target date не в майбутньому
        var maxAllowedDate = DateTime.UtcNow.Date.AddDays(1);
        if (targetDate.Date > maxAllowedDate)
        {
            throw new InvalidOperationException(
                ValidationConstants.NotFutureDateError);
        }

        // Отримуємо записи за вихідний день
        var sourceEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId && te.EntryDate.Date == sourceDate.Date)
            .ToListAsync();

        if (!sourceEntries.Any())
        {
            _logger.LogInformation(
                "Немає записів для копіювання за дату {SourceDate} для користувача {UserId}",
                sourceDate, userId);
            return Enumerable.Empty<TimeEntryDto>();
        }

        // Перевіряємо чи вистачить місця для копіювання
        var totalHoursSource = sourceEntries.Sum(e => e.HoursMilliseconds);
        var existingHoursTarget = await _validationService.GetTotalHoursForDayAsync(userId, targetDate);

        if (existingHoursTarget + totalHoursSource > ValidationConstants.MaxHoursPerDayMs)
        {
            throw new InvalidOperationException(
                $"Неможливо скопіювати записи. " +
                $"За цільовою датою вже є {TimeHelper.FormatHours(existingHoursTarget)}, " +
                $"копіюється {TimeHelper.FormatHours(totalHoursSource)}, " +
                $"що перевищить ліміт 24:00");
        }

        var copiedEntries = new List<TimeEntry>();

        try
        {
            foreach (var sourceEntry in sourceEntries)
            {
                var newEntry = new TimeEntry
                {
                    UserId = userId,
                    EntryDate = targetDate.Date,
                    AgencyId = sourceEntry.AgencyId,
                    MarketId = sourceEntry.MarketId,
                    ContractingAgencyId = sourceEntry.ContractingAgencyId,
                    ClientId = sourceEntry.ClientId,
                    ProjectBrandId = sourceEntry.ProjectBrandId,
                    MediaId = sourceEntry.MediaId,
                    JobTypeId = sourceEntry.JobTypeId,
                    HoursMilliseconds = sourceEntry.HoursMilliseconds,
                    Comments = $"Скопійовано з {sourceDate:yyyy-MM-dd}" +
                               (string.IsNullOrEmpty(sourceEntry.Comments)
                                   ? ""
                                   : $": {sourceEntry.Comments}")
                };

                await _timeEntryRepository.AddAsync(newEntry);
                copiedEntries.Add(newEntry);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Копіювання дня завершено. UserId: {UserId}, SourceDate: {SourceDate}, " +
                "TargetDate: {TargetDate}, Count: {Count}",
                userId, sourceDate, targetDate, copiedEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Помилка при копіюванні дня для користувача {UserId}",
                userId);
            throw;
        }

        // Завантажуємо скопійовані записи
        var copiedIds = copiedEntries.Select(e => e.Id).ToList();
        var entriesWithDetails = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Where(te => copiedIds.Contains(te.Id))
            .ToListAsync();

        return _mapper.Map<IEnumerable<TimeEntryDto>>(entriesWithDetails);
    }

    public async Task<IEnumerable<TimeEntryDto>> CopyWeekEntriesAsync(
        long userId,
        DateTime sourceWeekStart,
        DateTime targetWeekStart,
        long requestingUserId)
    {
        // Перевірка прав
        var permissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            userId);

        if (!permissionResult.IsValid)
        {
            throw new UnauthorizedAccessException(
                string.Join("; ", permissionResult.Errors));
        }

        // Нормалізуємо дати до понеділка
        var sourceMonday = sourceWeekStart.Date.AddDays(-(int)sourceWeekStart.DayOfWeek + (int)DayOfWeek.Monday);
        var targetMonday = targetWeekStart.Date.AddDays(-(int)targetWeekStart.DayOfWeek + (int)DayOfWeek.Monday);
        var allCopiedEntries = new List<TimeEntryDto>();

        // Копіюємо кожен день тижня
        for (int i = 0; i < 5; i++) // Тільки робочі дні (Пн-Пт)
        {
            var sourceDay = sourceMonday.AddDays(i);
            var targetDay = targetMonday.AddDays(i);

            try
            {
                var copiedDayEntries = await CopyDayEntriesAsync(
                    userId,
                    sourceDay,
                    targetDay,
                    requestingUserId);

                allCopiedEntries.AddRange(copiedDayEntries);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    "Не вдалося скопіювати день {SourceDay} -> {TargetDay}: {Error}",
                    sourceDay, targetDay, ex.Message);
                // Продовжуємо копіювання інших днів
            }
        }

        _logger.LogInformation(
            "Копіювання тижня завершено. UserId: {UserId}, SourceWeek: {SourceWeek}, " +
            "TargetWeek: {TargetWeek}, TotalCopied: {Count}",
            userId, sourceMonday, targetMonday, allCopiedEntries.Count);

        return allCopiedEntries;
    }

    public async Task<object> GetDailySummaryAsync(long userId, DateTime date)
    {
        var summary = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId && te.EntryDate.Date == date.Date)
            .GroupBy(te => te.EntryDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalHoursMs = g.Sum(te => te.HoursMilliseconds),
                TotalEntries = g.Count(),
                Entries = g.Select(te => new
                {
                    te.Id,
                    ClientName = te.Client.Name,
                    ProjectName = te.ProjectBrand.Name,
                    te.HoursMilliseconds,
                    FormattedHours = TimeHelper.FormatHours(te.HoursMilliseconds),
                    te.Comments
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (summary == null)
        {
            return new
            {
                Date = date.Date,
                TotalHoursMs = 0L,
                TotalHours = "00:00",
                TotalEntries = 0,
                RemainingHoursMs = ValidationConstants.MaxHoursPerDayMs,
                RemainingHours = "24:00",
                Entries = new List<object>()
            };
        }

        var remaining = ValidationConstants.MaxHoursPerDayMs - summary.TotalHoursMs;

        return new
        {
            summary.Date,
            summary.TotalHoursMs,
            TotalHours = TimeHelper.FormatHours(summary.TotalHoursMs),
            summary.TotalEntries,
            RemainingHoursMs = remaining,
            RemainingHours = TimeHelper.FormatHours(remaining),
            summary.Entries
        };
    }

    public async Task<object> GetWeeklySummaryAsync(long userId, DateTime weekStart)
    {
        var monday = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek + (int)DayOfWeek.Monday);
        var sunday = monday.AddDays(6);

        var weekEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId &&
                         te.EntryDate >= monday &&
                         te.EntryDate <= sunday)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .ToListAsync();

        var dailySummaries = weekEntries
            .GroupBy(te => te.EntryDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                DayOfWeek = g.Key.DayOfWeek,
                TotalHoursMs = g.Sum(te => te.HoursMilliseconds),
                TotalHours = TimeHelper.FormatHours(g.Sum(te => te.HoursMilliseconds)),
                EntriesCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        var totalWeekHours = weekEntries.Sum(e => e.HoursMilliseconds);

        return new
        {
            WeekStart = monday,
            WeekEnd = sunday,
            TotalHoursMs = totalWeekHours,
            TotalHours = TimeHelper.FormatHours(totalWeekHours),
            TotalEntries = weekEntries.Count,
            DailySummaries = dailySummaries,
            TopClients = weekEntries
                .GroupBy(e => new { e.ClientId, e.Client.Name })
                .Select(g => new
                {
                    g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    TotalHours = TimeHelper.FormatHours(g.Sum(e => e.HoursMilliseconds))
                })
                .OrderByDescending(c => c.TotalHoursMs)
                .Take(5)
                .ToList()
        };
    }

    public async Task<object> GetMonthlySummaryAsync(long userId, int year, int month)
    {
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var monthEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId &&
                         te.EntryDate >= firstDay &&
                         te.EntryDate <= lastDay)
            .Include(te => te.Client)
            .Include(te => te.ProjectBrand)
            .Include(te => te.JobType)
            .ToListAsync();

        var weekSummaries = monthEntries
            .GroupBy(te =>
            {
                var weekStart = te.EntryDate.AddDays(-(int)te.EntryDate.DayOfWeek + (int)DayOfWeek.Monday);
                return weekStart;
            })
            .Select(g => new
            {
                WeekStart = g.Key,
                TotalHoursMs = g.Sum(te => te.HoursMilliseconds),
                TotalHours = TimeHelper.FormatHours(g.Sum(te => te.HoursMilliseconds)),
                EntriesCount = g.Count()
            })
            .OrderBy(w => w.WeekStart)
            .ToList();

        var totalMonthHours = monthEntries.Sum(e => e.HoursMilliseconds);
        var workingDaysWithEntries = monthEntries.Select(e => e.EntryDate.Date).Distinct().Count();

        return new
        {
            Year = year,
            Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMMM yyyy"),
            TotalHoursMs = totalMonthHours,
            TotalHours = TimeHelper.FormatHours(totalMonthHours),
            TotalEntries = monthEntries.Count,
            WorkingDaysWithEntries = workingDaysWithEntries,
            AverageHoursPerDay = workingDaysWithEntries > 0
                ? TimeHelper.FormatHours(totalMonthHours / workingDaysWithEntries)
                : "00:00",
            WeekSummaries = weekSummaries,
            TopClients = monthEntries
                .GroupBy(e => new { e.ClientId, e.Client.Name })
                .Select(g => new
                {
                    g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    TotalHours = TimeHelper.FormatHours(g.Sum(e => e.HoursMilliseconds)),
                    Percentage = totalMonthHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalMonthHours * 100, 1)
                        : 0
                })
                .OrderByDescending(c => c.TotalHoursMs)
                .Take(10)
                .ToList(),
            TopJobTypes = monthEntries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new
                {
                    g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    TotalHours = TimeHelper.FormatHours(g.Sum(e => e.HoursMilliseconds)),
                    Percentage = totalMonthHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalMonthHours * 100, 1)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .Take(10)
                .ToList()
        };
    }

    public async Task<bool> CanUserEditEntryAsync(long entryId, long requestingUserId)
    {
        var entry = await _timeEntryRepository.GetByIdAsync(entryId);
        if (entry == null)
            return false;

        // Власник може редагувати свої записи
        if (entry.UserId == requestingUserId)
            return true;

        // Manager або Admin можуть редагувати чужі записи
        return await HasManagerOrAdminRoleAsync(requestingUserId);
    }

    public async Task<long> GetRemainingHoursForDayAsync(long userId, DateTime date, long? excludeEntryId = null)
    {
        var totalHours = excludeEntryId.HasValue
            ? await _validationService.GetTotalHoursForDayAsync(userId, date, excludeEntryId.Value)
            : await _validationService.GetTotalHoursForDayAsync(userId, date);

        return ValidationConstants.MaxHoursPerDayMs - totalHours;
    }

    private async Task<bool> CanUserViewEntryAsync(TimeEntry entry, long requestingUserId)
    {
        // Власник може переглядати свої записи
        if (entry.UserId == requestingUserId)
            return true;

        // Manager або Admin можуть переглядати чужі записи
        return await HasManagerOrAdminRoleAsync(requestingUserId);
    }

    private async Task<bool> HasManagerOrAdminRoleAsync(long userId)
    {
        var userWithRoles = await _userRepository.GetByIdWithRolesAsync(userId);
        if (userWithRoles == null)
            return false;

        var roles = userWithRoles.UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToList();

        return roles.Contains("Admin") || roles.Contains("Manager");
    }
}