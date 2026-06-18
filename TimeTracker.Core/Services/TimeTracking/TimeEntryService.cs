using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Core.Services.Audit;
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
    private readonly IAuditService _auditService;

    public TimeEntryService(
        ITimeEntryRepository timeEntryRepository,
        IUserRepository userRepository,
        ITimeValidationService validationService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TimeEntryService> logger,
        IAuditService auditService)

    {
        _timeEntryRepository = timeEntryRepository;
        _userRepository = userRepository;
        _validationService = validationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _auditService = auditService;
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
            .Include(te => te.Department)
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
        return _mapper.Map<IEnumerable<TimeEntryListItemDto>>(entries);
    }

    public async Task<TimeEntryDto> CreateAsync(
        CreateTimeEntryDto dto,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent)
    {
        // 1. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            dto.UserId);

        if (!userPermissionResult.IsValid)
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався створити TimeEntry для UserId {TargetUserId}. Errors: {Errors}",
                requestingUserId, dto.UserId, string.Join("; ", userPermissionResult.Errors));
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
            _logger.LogWarning(
                "Валідація створення TimeEntry для UserId {UserId} не пройдена. Errors: {Errors}",
                dto.UserId, string.Join("; ", createValidationResult.Errors));
            throw new InvalidOperationException(
                string.Join("; ", createValidationResult.Errors));
        }

        // 3. Валідація зовнішніх ключів
        var referencesValidationResult = await _validationService.ValidateReferencesAsync(
            agencyId,
            dto.MarketId,
            dto.ContractingAgencyId,
            dto.ClientId,
            dto.MediaId,
            dto.JobTypeId);

        if (!referencesValidationResult.IsValid)
        {
            _logger.LogWarning(
                "Валідація посилань для TimeEntry не пройдена. Errors: {Errors}",
                string.Join("; ", referencesValidationResult.Errors));
            throw new InvalidOperationException(
                string.Join("; ", referencesValidationResult.Errors));
        }

        // 4. Отримуємо DepartmentId з User
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {dto.UserId} не знайдено");

        // 5. Створення запису
        var timeEntry = _mapper.Map<TimeEntry>(dto);
        timeEntry.AgencyId = agencyId;
        timeEntry.DepartmentId = user.DepartmentId;

        await _timeEntryRepository.AddAsync(timeEntry);
        await _unitOfWork.SaveChangesAsync();

        // 6. Аудит (оставь как есть)
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        var targetUser = user;

        if (requestingUser != null && targetUser != null)
        {
            var entryDetails = new
            {
                agencyId,
                dto.MarketId,
                dto.ContractingAgencyId,
                dto.ClientId,
                dto.ProjectBrand,
                dto.MediaId,
                dto.JobTypeId,
                dto.Comments
            };

            await _auditService.LogTimeEntryCreatedAsync(
                timeEntryId: timeEntry.Id,
                userId: timeEntry.UserId,
                userName: targetUser.Name,
                entryDate: timeEntry.EntryDate,
                hoursMilliseconds: timeEntry.HoursMilliseconds,
                entryDetails: entryDetails,
                createdByUserId: requestingUserId,
                createdByUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        else
        {
            _logger.LogError(
                "Не вдалося знайти користувачів для аудиту створення TimeEntry. RequestingUserId: {RequestingUserId}, TargetUserId: {TargetUserId}",
                requestingUserId, dto.UserId);
        }

        return _mapper.Map<TimeEntryDto>(timeEntry);
    }

    public async Task<TimeEntryDto> UpdateAsync(
        long id,
        UpdateTimeEntryDto dto,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent)
    {
        // 1. Получаем запись с включением связанных данных
        var timeEntry = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (timeEntry == null)
        {
            _logger.LogWarning(
                "Спроба оновлення неіснуючого TimeEntry з ID {Id}",
                id);
            throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
        }

        // 2. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            timeEntry.UserId);

        if (!userPermissionResult.IsValid)
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався оновити TimeEntry {EntryId}, який не належить йому. Errors: {Errors}",
                requestingUserId, id, string.Join("; ", userPermissionResult.Errors));
            throw new UnauthorizedAccessException(
                string.Join("; ", userPermissionResult.Errors));
        }

        // 3. Зберігаємо старі значення для аудиту
        var oldValues = new
        {
            timeEntry.Id,
            timeEntry.UserId,
            UserName = timeEntry.User.Name,
            timeEntry.EntryDate,
            timeEntry.HoursMilliseconds,
            timeEntry.AgencyId,
            timeEntry.MarketId,
            timeEntry.ContractingAgencyId,
            timeEntry.ClientId,
            timeEntry.ProjectBrand,
            timeEntry.MediaId,
            timeEntry.JobTypeId,
            timeEntry.Comments
        };

        // 4. Валідація даних оновлення
        var updateValidationResult = await _validationService.ValidateUpdateAsync(
            id,
            timeEntry.UserId,
            dto.EntryDate,
            dto.HoursMilliseconds);

        if (!updateValidationResult.IsValid)
        {
            _logger.LogWarning(
                "Валідація оновлення TimeEntry {Id} не пройдена. Errors: {Errors}",
                id, string.Join("; ", updateValidationResult.Errors));
            throw new InvalidOperationException(
                string.Join("; ", updateValidationResult.Errors));
        }

        // 5. Валідація зовнішніх ключів
        var referencesValidationResult = await _validationService.ValidateReferencesAsync(
            agencyId,
            dto.MarketId,
            dto.ContractingAgencyId,
            dto.ClientId,
            dto.MediaId,
            dto.JobTypeId);

        if (!referencesValidationResult.IsValid)
        {
            _logger.LogWarning(
                "Валідація посилань для TimeEntry {Id} не пройдена. Errors: {Errors}",
                id, string.Join("; ", referencesValidationResult.Errors));
            throw new InvalidOperationException(
                string.Join("; ", referencesValidationResult.Errors));
        }

        // 6. Оновлення entity
        _mapper.Map(dto, timeEntry);
        timeEntry.AgencyId = agencyId;

        _timeEntryRepository.Update(timeEntry);
        await _unitOfWork.SaveChangesAsync();

        // 7. АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        var targetUser = await _userRepository.GetByIdAsync(timeEntry.UserId);

        if (requestingUser != null && targetUser != null)
        {
            var newValues = new
            {
                timeEntry.Id,
                timeEntry.UserId,
                UserName = targetUser.Name,
                timeEntry.EntryDate,
                timeEntry.HoursMilliseconds,
                timeEntry.AgencyId,
                timeEntry.MarketId,
                timeEntry.ContractingAgencyId,
                timeEntry.ClientId,
                timeEntry.ProjectBrand,
                timeEntry.MediaId,
                timeEntry.JobTypeId,
                timeEntry.Comments,
                UpdatedBy = requestingUser.Name
            };

            await _auditService.LogTimeEntryUpdatedAsync(
                timeEntryId: timeEntry.Id,
                userId: timeEntry.UserId,
                userName: targetUser.Name,
                oldValues: oldValues,
                newValues: newValues,
                updatedByUserId: requestingUserId,
                updatedByUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        else
        {
            _logger.LogError(
                "Не вдалося знайти користувачів для аудиту оновлення TimeEntry. RequestingUserId: {RequestingUserId}, TargetUserId: {TargetUserId}",
                requestingUserId, timeEntry.UserId);
        }

        return _mapper.Map<TimeEntryDto>(timeEntry);
    }

    public async Task DeleteAsync(
        long id,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        // 1. Получаем запись с включением связанных данных
        var timeEntry = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (timeEntry == null)
        {
            _logger.LogWarning(
                "Спроба видалення неіснуючого TimeEntry з ID {Id}",
                id);
            throw new KeyNotFoundException($"TimeEntry з ID {id} не знайдено");
        }

        // 2. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            timeEntry.UserId);

        if (!userPermissionResult.IsValid)
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався видалити TimeEntry {EntryId}, який не належить йому. Errors: {Errors}",
                requestingUserId, id, string.Join("; ", userPermissionResult.Errors));
            throw new UnauthorizedAccessException(
                string.Join("; ", userPermissionResult.Errors));
        }

        // 3. Зберігаємо дані для аудиту перед видаленням
        var oldValues = new
        {
            timeEntry.Id,
            timeEntry.UserId,
            UserName = timeEntry.User.Name,
            timeEntry.EntryDate,
            timeEntry.HoursMilliseconds,
            timeEntry.AgencyId,
            timeEntry.MarketId,
            timeEntry.ContractingAgencyId,
            timeEntry.ClientId,
            timeEntry.ProjectBrand,
            timeEntry.MediaId,
            timeEntry.JobTypeId,
            timeEntry.Comments
        };

        // 4. Зберігаємо дані користувачів перед видаленням entity
        var targetUserId = timeEntry.UserId;
        var targetUserName = timeEntry.User.Name;

        // 5. Видалення
        await _timeEntryRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        // 6. АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);

        if (requestingUser != null)
        {
            await _auditService.LogTimeEntryDeletedAsync(
                timeEntryId: id,
                userId: targetUserId,
                userName: targetUserName,
                oldValues: oldValues,
                deletedByUserId: requestingUserId,
                deletedByUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        else
        {
            _logger.LogError(
                "Не вдалося знайти користувача для аудиту видалення TimeEntry. RequestingUserId: {RequestingUserId}",
                requestingUserId);
        }
    }

    public async Task<(IEnumerable<TimeEntryListItemDto> Entries, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        long? userId = null,
        long? agencyId = null,
        long? clientId = null,
        long? departmentId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        long requestingUserId = 0)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 200) pageSize = 200;

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

        // Один запрос — з Include вже всередині репозиторію
        var (entries, totalCount) = await _timeEntryRepository.GetTimeEntriesPagedAsync(
            pageNumber,
            pageSize,
            userId,
            agencyId,
            clientId,
            departmentId,
            fromDate,
            toDate);

        var dtos = _mapper.Map<IEnumerable<TimeEntryListItemDto>>(entries);

        return (dtos, totalCount);
    }

    public async Task<IEnumerable<TimeEntryDto>> CreateBulkAsync(
        IEnumerable<CreateTimeEntryDto> dtos,
        long requestingUserId,
        long agencyId,
        string ipAddress,
        string userAgent)
    {
        var dtosList = dtos.ToList();

        if (!dtosList.Any())
        {
            _logger.LogWarning("Спроба масового створення порожнього списку TimeEntries");
            return new List<TimeEntryDto>();
        }

        // Валідація прав для кожного запису
        foreach (var dto in dtosList)
        {
            var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
                requestingUserId,
                dto.UserId);

            if (!userPermissionResult.IsValid)
            {
                _logger.LogWarning(
                    "Користувач {RequestingUserId} намагався масово створити TimeEntry для UserId {TargetUserId}",
                    requestingUserId, dto.UserId);
                throw new UnauthorizedAccessException(
                    $"Немає прав для створення записів користувача {dto.UserId}");
            }
        }

        // Валідація всіх записів
        foreach (var dto in dtosList)
        {
            var createValidationResult = await _validationService.ValidateCreateAsync(
                dto.UserId,
                dto.EntryDate,
                dto.HoursMilliseconds);

            if (!createValidationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Валідація не пройдена для запису: {string.Join("; ", createValidationResult.Errors)}");
            }

            var referencesValidationResult = await _validationService.ValidateReferencesAsync(
                agencyId,
                dto.MarketId,
                dto.ContractingAgencyId,
                dto.ClientId,
                dto.MediaId,
                dto.JobTypeId);

            if (!referencesValidationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Валідація посилань не пройдена: {string.Join("; ", referencesValidationResult.Errors)}");
            }
        }

        // Створення записів
        var timeEntries = dtosList.Select(dto =>
        {
            var entry = _mapper.Map<TimeEntry>(dto);
            entry.AgencyId = agencyId; // явно з токена
            return entry;
        }).ToList();

        await _timeEntryRepository.AddRangeAsync(timeEntries);
        await _unitOfWork.SaveChangesAsync();

        // Аудит
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        var groupedByUser = timeEntries.GroupBy(te => te.UserId);

        foreach (var userGroup in groupedByUser)
        {
            var targetUser = await _userRepository.GetByIdAsync(userGroup.Key);

            if (requestingUser != null && targetUser != null)
            {
                await _auditService.LogTimeEntriesBulkOperationAsync(
                    operation: AuditAction.BulkCreate,
                    userId: userGroup.Key,
                    userName: targetUser.Name,
                    affectedCount: userGroup.Count(),
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }

        return timeEntries.Select(te => _mapper.Map<TimeEntryDto>(te));
    }

    public async Task<IEnumerable<TimeEntryDto>> UpdateBulkAsync(
        IEnumerable<(long Id, UpdateTimeEntryDto Dto)> updates,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var updatesList = updates.ToList();

        if (!updatesList.Any())
        {
            _logger.LogWarning("Спроба масового оновлення порожнього списку TimeEntries");
            return new List<TimeEntryDto>();
        }

        var ids = updatesList.Select(u => u.Id).ToList();
        var timeEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => ids.Contains(te.Id))
            .ToListAsync();

        if (timeEntries.Count != updatesList.Count)
        {
            throw new KeyNotFoundException("Деякі TimeEntry не знайдено");
        }

        // Валідація прав для всіх записів
        foreach (var entry in timeEntries)
        {
            var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
                requestingUserId,
                entry.UserId);

            if (!userPermissionResult.IsValid)
            {
                throw new UnauthorizedAccessException(
                    $"Немає прав для оновлення запису {entry.Id}");
            }
        }

        // Оновлення записів
        foreach (var update in updatesList)
        {
            var entry = timeEntries.First(te => te.Id == update.Id);

            var updateValidationResult = await _validationService.ValidateUpdateAsync(
                update.Id,
                entry.UserId,
                update.Dto.EntryDate,
                update.Dto.HoursMilliseconds);

            if (!updateValidationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Валідація не пройдена для запису {update.Id}");
            }

            _mapper.Map(update.Dto, entry);
        }

        _timeEntryRepository.UpdateRange(timeEntries);
        await _unitOfWork.SaveChangesAsync();

        // АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);

        var groupedByUser = timeEntries.GroupBy(te => te.UserId);

        foreach (var userGroup in groupedByUser)
        {
            var targetUser = await _userRepository.GetByIdAsync(userGroup.Key);

            if (requestingUser != null && targetUser != null)
            {
                await _auditService.LogTimeEntriesBulkOperationAsync(
                    operation: AuditAction.BulkUpdate,
                    userId: userGroup.Key,
                    userName: targetUser.Name,
                    affectedCount: userGroup.Count(),
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }

        return timeEntries.Select(te => _mapper.Map<TimeEntryDto>(te));
    }

    public async Task DeleteBulkAsync(
        IEnumerable<long> ids,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        var idsList = ids.ToList();

        if (!idsList.Any())
        {
            _logger.LogWarning("Спроба масового видалення порожнього списку TimeEntries");
            return;
        }

        var timeEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => idsList.Contains(te.Id))
            .ToListAsync();

        if (timeEntries.Count != idsList.Count)
        {
            throw new KeyNotFoundException("Деякі TimeEntry не знайдено");
        }

        // Валідація прав для всіх записів
        foreach (var entry in timeEntries)
        {
            var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
                requestingUserId,
                entry.UserId);

            if (!userPermissionResult.IsValid)
            {
                throw new UnauthorizedAccessException(
                    $"Немає прав для видалення запису {entry.Id}");
            }
        }

        // Зберігаємо інформацію для аудиту перед видаленням
        var groupedByUser = timeEntries.GroupBy(te => te.UserId).ToList();

        _timeEntryRepository.DeleteRange(timeEntries);
        await _unitOfWork.SaveChangesAsync();

        // АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);

        foreach (var userGroup in groupedByUser)
        {
            var targetUser = await _userRepository.GetByIdAsync(userGroup.Key);

            if (requestingUser != null && targetUser != null)
            {
                await _auditService.LogTimeEntriesBulkOperationAsync(
                    operation: AuditAction.BulkDelete,
                    userId: userGroup.Key,
                    userName: targetUser.Name,
                    affectedCount: userGroup.Count(),
                    requestingUserId: requestingUserId,
                    requestingUserName: requestingUser.Name,
                    ipAddress: ipAddress,
                    userAgent: userAgent);
            }
        }
    }

    public async Task<IEnumerable<TimeEntryDto>> CopyDayEntriesAsync(
        long userId,
        DateTime sourceDate,
        DateTime targetDate,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        // 1. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            userId);

        if (!userPermissionResult.IsValid)
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався скопіювати записи для UserId {TargetUserId}",
                requestingUserId, userId);
            throw new UnauthorizedAccessException(
                string.Join("; ", userPermissionResult.Errors));
        }

        // 2. Перевірка, що дати різні
        if (sourceDate.Date == targetDate.Date)
        {
            _logger.LogWarning(
                "Спроба копіювання записів на ту саму дату. UserId: {UserId}, Date: {Date}",
                userId, sourceDate.Date);
            throw new InvalidOperationException("Неможливо скопіювати записи на ту саму дату");
        }

        // 3. Перевірка, що targetDate не в майбутньому
        if (targetDate.Date > DateTime.UtcNow.Date)
        {
            _logger.LogWarning(
                "Спроба копіювання записів на майбутню дату. UserId: {UserId}, TargetDate: {TargetDate}",
                userId, targetDate.Date);
            throw new InvalidOperationException("Неможливо скопіювати записи на майбутню дату");
        }

        // 4. Отримуємо записи з source дати
        var sourceEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId && te.EntryDate.Date == sourceDate.Date)
            .ToListAsync();

        if (!sourceEntries.Any())
        {
            _logger.LogInformation(
                "Немає записів для копіювання. UserId: {UserId}, SourceDate: {SourceDate}",
                userId, sourceDate.Date);
            return new List<TimeEntryDto>();
        }

        // 5. Перевіряємо чи є вже записи на target дату
        var existingEntriesOnTarget = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId && te.EntryDate.Date == targetDate.Date)
            .ToListAsync();

        if (existingEntriesOnTarget.Any())
        {
            _logger.LogWarning(
                "На цільову дату вже існують записи. UserId: {UserId}, TargetDate: {TargetDate}, Count: {Count}",
                userId, targetDate.Date, existingEntriesOnTarget.Count);
            throw new InvalidOperationException(
                $"На дату {targetDate.Date:yyyy-MM-dd} вже існують записи ({existingEntriesOnTarget.Count}). Спочатку видаліть їх.");
        }

        // 6. Створюємо нові записи на основі source
        var newEntries = new List<TimeEntry>();

        foreach (var sourceEntry in sourceEntries)
        {
            // Валідація кожного нового запису
            var createValidationResult = await _validationService.ValidateCreateAsync(
                userId,
                targetDate,
                sourceEntry.HoursMilliseconds);

            if (!createValidationResult.IsValid)
            {
                _logger.LogWarning(
                    "Валідація копіювання запису не пройдена. SourceEntryId: {SourceId}, Errors: {Errors}",
                    sourceEntry.Id, string.Join("; ", createValidationResult.Errors));
                throw new InvalidOperationException(
                    $"Не вдалося скопіювати запис: {string.Join("; ", createValidationResult.Errors)}");
            }

            var newEntry = new TimeEntry
            {
                UserId = userId,
                EntryDate = targetDate.Date,
                AgencyId = sourceEntry.AgencyId,
                MarketId = sourceEntry.MarketId,
                ContractingAgencyId = sourceEntry.ContractingAgencyId,
                ClientId = sourceEntry.ClientId,
                ProjectBrand = sourceEntry.ProjectBrand,
                MediaId = sourceEntry.MediaId,
                JobTypeId = sourceEntry.JobTypeId,
                HoursMilliseconds = sourceEntry.HoursMilliseconds,
                Comments = sourceEntry.Comments
            };

            newEntries.Add(newEntry);
        }

        // 7. Зберігаємо нові записи
        await _timeEntryRepository.AddRangeAsync(newEntries);
        await _unitOfWork.SaveChangesAsync();

        // 8. АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        var targetUser = await _userRepository.GetByIdAsync(userId);

        if (requestingUser != null && targetUser != null)
        {
            await _auditService.LogTimeEntriesCopiedAsync(
                userId: userId,
                userName: targetUser.Name,
                sourceDate: sourceDate.Date,
                targetDate: targetDate.Date,
                copiedCount: newEntries.Count,
                copyType: "Day",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        else
        {
            _logger.LogError(
                "Не вдалося знайти користувачів для аудиту копіювання. RequestingUserId: {RequestingUserId}, TargetUserId: {TargetUserId}",
                requestingUserId, userId);
        }

        return newEntries.Select(te => _mapper.Map<TimeEntryDto>(te));
    }

    public async Task<IEnumerable<TimeEntryDto>> CopyWeekEntriesAsync(
        long userId,
        DateTime sourceWeekStart,
        DateTime targetWeekStart,
        long requestingUserId,
        string ipAddress,
        string userAgent)
    {
        // 1. Валідація прав користувача
        var userPermissionResult = await _validationService.ValidateUserPermissionsAsync(
            requestingUserId,
            userId);

        if (!userPermissionResult.IsValid)
        {
            _logger.LogWarning(
                "Користувач {RequestingUserId} намагався скопіювати тижневі записи для UserId {TargetUserId}",
                requestingUserId, userId);
            throw new UnauthorizedAccessException(
                string.Join("; ", userPermissionResult.Errors));
        }

        // 2. Нормалізуємо дати до початку тижня (понеділок)
        var normalizedSourceStart =
            sourceWeekStart.Date.AddDays(-(int)sourceWeekStart.DayOfWeek + (int)DayOfWeek.Monday);
        var normalizedTargetStart =
            targetWeekStart.Date.AddDays(-(int)targetWeekStart.DayOfWeek + (int)DayOfWeek.Monday);

        // Якщо неділя, то це насправді попередній тиждень
        if (sourceWeekStart.DayOfWeek == DayOfWeek.Sunday)
        {
            normalizedSourceStart = normalizedSourceStart.AddDays(-7);
        }

        if (targetWeekStart.DayOfWeek == DayOfWeek.Sunday)
        {
            normalizedTargetStart = normalizedTargetStart.AddDays(-7);
        }

        var sourceWeekEnd = normalizedSourceStart.AddDays(6); // Неділя
        var targetWeekEnd = normalizedTargetStart.AddDays(6);

        // 3. Перевірка, що тижні різні
        if (normalizedSourceStart == normalizedTargetStart)
        {
            _logger.LogWarning(
                "Спроба копіювання записів на той самий тиждень. UserId: {UserId}, WeekStart: {WeekStart}",
                userId, normalizedSourceStart);
            throw new InvalidOperationException("Неможливо скопіювати записи на той самий тиждень");
        }

        // 4. Перевірка, що targetWeek не в майбутньому
        if (normalizedTargetStart > DateTime.UtcNow.Date)
        {
            _logger.LogWarning(
                "Спроба копіювання записів на майбутній тиждень. UserId: {UserId}, TargetWeekStart: {TargetWeekStart}",
                userId, normalizedTargetStart);
            throw new InvalidOperationException("Неможливо скопіювати записи на майбутній тиждень");
        }

        // 5. Отримуємо всі записи з source тижня
        var sourceEntries = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId
                         && te.EntryDate.Date >= normalizedSourceStart
                         && te.EntryDate.Date <= sourceWeekEnd)
            .OrderBy(te => te.EntryDate)
            .ToListAsync();

        if (!sourceEntries.Any())
        {
            _logger.LogInformation(
                "Немає записів для копіювання. UserId: {UserId}, SourceWeekStart: {SourceWeekStart}",
                userId, normalizedSourceStart);
            return new List<TimeEntryDto>();
        }

        // 6. Перевіряємо чи є вже записи на target тиждень
        var existingEntriesOnTarget = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId
                         && te.EntryDate.Date >= normalizedTargetStart
                         && te.EntryDate.Date <= targetWeekEnd)
            .ToListAsync();

        if (existingEntriesOnTarget.Any())
        {
            _logger.LogWarning(
                "На цільовий тиждень вже існують записи. UserId: {UserId}, TargetWeekStart: {TargetWeekStart}, Count: {Count}",
                userId, normalizedTargetStart, existingEntriesOnTarget.Count);
            throw new InvalidOperationException(
                $"На тиждень з {normalizedTargetStart:yyyy-MM-dd} вже існують записи ({existingEntriesOnTarget.Count}). Спочатку видаліть їх.");
        }

        // 7. Створюємо нові записи на основі source
        var newEntries = new List<TimeEntry>();

        foreach (var sourceEntry in sourceEntries)
        {
            // Вираховуємо різницю днів між source та початком source тижня
            var dayOffset = (sourceEntry.EntryDate.Date - normalizedSourceStart).Days;

            // Додаємо цю різницю до target початку тижня
            var newEntryDate = normalizedTargetStart.AddDays(dayOffset);

            // Перевіряємо, що нова дата не в майбутньому
            if (newEntryDate > DateTime.UtcNow.Date)
            {
                _logger.LogWarning(
                    "Пропускаємо копіювання запису на майбутню дату. SourceEntryId: {SourceId}, TargetDate: {TargetDate}",
                    sourceEntry.Id, newEntryDate);
                continue; // Пропускаємо майбутні дати
            }

            // Валідація кожного нового запису
            var createValidationResult = await _validationService.ValidateCreateAsync(
                userId,
                newEntryDate,
                sourceEntry.HoursMilliseconds);

            if (!createValidationResult.IsValid)
            {
                _logger.LogWarning(
                    "Валідація копіювання запису не пройдена. SourceEntryId: {SourceId}, NewDate: {NewDate}, Errors: {Errors}",
                    sourceEntry.Id, newEntryDate, string.Join("; ", createValidationResult.Errors));

                // Для тижневого копіювання продовжуємо, але логуємо помилку
                continue;
            }

            var newEntry = new TimeEntry
            {
                UserId = userId,
                EntryDate = newEntryDate,
                AgencyId = sourceEntry.AgencyId,
                MarketId = sourceEntry.MarketId,
                ContractingAgencyId = sourceEntry.ContractingAgencyId,
                ClientId = sourceEntry.ClientId,
                ProjectBrand = sourceEntry.ProjectBrand,
                MediaId = sourceEntry.MediaId,
                JobTypeId = sourceEntry.JobTypeId,
                HoursMilliseconds = sourceEntry.HoursMilliseconds,
                Comments = sourceEntry.Comments
            };

            newEntries.Add(newEntry);
        }

        if (!newEntries.Any())
        {
            _logger.LogWarning(
                "Жоден запис не пройшов валідацію для копіювання. UserId: {UserId}",
                userId);
            throw new InvalidOperationException("Не вдалося скопіювати жоден запис через помилки валідації");
        }

        // 8. Зберігаємо нові записи
        await _timeEntryRepository.AddRangeAsync(newEntries);
        await _unitOfWork.SaveChangesAsync();

        // 9. АУДИТ В БД - бізнес-логіка
        var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
        var targetUser = await _userRepository.GetByIdAsync(userId);

        if (requestingUser != null && targetUser != null)
        {
            await _auditService.LogTimeEntriesCopiedAsync(
                userId: userId,
                userName: targetUser.Name,
                sourceDate: normalizedSourceStart,
                targetDate: normalizedTargetStart,
                copiedCount: newEntries.Count,
                copyType: "Week",
                requestingUserId: requestingUserId,
                requestingUserName: requestingUser.Name,
                ipAddress: ipAddress,
                userAgent: userAgent);
        }
        else
        {
            _logger.LogError(
                "Не вдалося знайти користувачів для аудиту копіювання тижня. RequestingUserId: {RequestingUserId}, TargetUserId: {TargetUserId}",
                requestingUserId, userId);
        }

        return newEntries.Select(te => _mapper.Map<TimeEntryDto>(te));
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
                    ProjectName = te.ProjectBrand,
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
            .Include(te => te.JobType)
            .AsNoTracking()
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