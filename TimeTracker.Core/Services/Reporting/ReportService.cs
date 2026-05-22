using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Reports;
using TimeTracker.Core.DTOs.Reports.Common;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;
using TimeTracker.Core.DTOs.TimeEntries;

namespace TimeTracker.Core.Services.Reporting;

public class ReportService : IReportService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ReportService> _logger;
    private readonly IMapper _mapper;

    // Настройки кеширования
    private const int CacheDurationMinutes = 15;
    private const string CacheKeyPrefix = "report:";

    public ReportService(
        ITimeEntryRepository timeEntryRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<ReportService> logger,
        IMapper mapper)
    {
        _timeEntryRepository = timeEntryRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimeEntryDto>> GetAllTimeEntriesForExportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        if (!await CanUserAccessReportAsync(requestingUserId))
            throw new UnauthorizedAccessException("Вы не имеете доступа к этому отчету");

        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User).ThenInclude(u => u.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date)
            .OrderBy(te => te.EntryDate)
            .ThenBy(te => te.User.Name)
            .AsNoTracking()
            .Select(te => new TimeEntryDto
            {
                Id = te.Id,
                UserId = te.UserId,
                UserName = te.User.Name,
                AgencyName = te.User.Agency != null ? te.User.Agency.Name : string.Empty,
                EntryDate = te.EntryDate,
                MarketName = te.Market != null ? te.Market.Name : string.Empty,
                ContractingAgencyName = te.ContractingAgency != null ? te.ContractingAgency.Name : string.Empty,
                ClientName = te.Client != null ? te.Client.Name : string.Empty,
                ProjectBrandName = te.ProjectBrand,
                MediaName = te.Media != null ? te.Media.Name : string.Empty,
                JobTypeName = te.JobType != null ? te.JobType.Name : string.Empty,
                HoursMilliseconds = te.HoursMilliseconds,
                Comments = te.Comments
            })
            .ToListAsync();

        return entries;
    }

    public async Task<IEnumerable<TimeEntryDto>> GetUserTimeEntriesForExportAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        if (!await CanUserAccessReportAsync(requestingUserId, userId))
            throw new UnauthorizedAccessException("Вы не имеете доступа к этому отчету");

        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User).ThenInclude(u => u.Agency)
            .Include(te => te.Market)
            .Include(te => te.ContractingAgency)
            .Include(te => te.Client)
            .Include(te => te.Media)
            .Include(te => te.JobType)
            .Where(te => te.UserId == userId
                         && te.EntryDate >= fromDate.Date
                         && te.EntryDate <= toDate.Date)
            .OrderBy(te => te.EntryDate)
            .AsNoTracking()
            .Select(te => new TimeEntryDto
            {
                Id = te.Id,
                UserId = te.UserId,
                UserName = te.User.Name,
                AgencyName = te.User.Agency != null ? te.User.Agency.Name : string.Empty,
                EntryDate = te.EntryDate,
                MarketName = te.Market != null ? te.Market.Name : string.Empty,
                ContractingAgencyName = te.ContractingAgency != null ? te.ContractingAgency.Name : string.Empty,
                ClientName = te.Client != null ? te.Client.Name : string.Empty,
                ProjectBrandName = te.ProjectBrand,
                MediaName = te.Media != null ? te.Media.Name : string.Empty,
                JobTypeName = te.JobType != null ? te.JobType.Name : string.Empty,
                HoursMilliseconds = te.HoursMilliseconds,
                Comments = te.Comments
            })
            .ToListAsync();

        return entries;
    }

    public async Task<UserLoadReportDto> GetUserLoadReportAsync(
        long userId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        // Проверка прав доступа
        if (!await CanUserAccessReportAsync(requestingUserId, userId))
        {
            _logger.LogWarning(
                "Пользователь {RequestingUserId} пытался получить отчет пользователя {UserId} без прав доступа",
                requestingUserId, userId);
            throw new UnauthorizedAccessException("Вы не имеете доступа к этому отчету");
        }

        // Проверка кеша
        var cacheKey = $"{CacheKeyPrefix}user-load:{userId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out UserLoadReportDto? cachedReport) && cachedReport != null)
        {
            return cachedReport;
        }

        // Получаем пользователя
        var user = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException($"Пользователь с ID {userId} не найден");
        }

        // Получаем записи времени за период
        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.Client)
            .Include(te => te.JobType)
            .Where(te => te.UserId == userId &&
                         te.EntryDate >= fromDate.Date &&
                         te.EntryDate <= toDate.Date)
            .AsNoTracking()
            .ToListAsync();

        // Вычисляем статистику
        var totalHoursMs = entries.Sum(e => e.HoursMilliseconds);
        var workingDays = entries.Select(e => e.EntryDate.Date).Distinct().Count();
        var periodDays = (toDate.Date - fromDate.Date).Days + 1;

        // Формируем отчет
        var report = new UserLoadReportDto
        {
            UserId = userId,
            UserName = user.Name,
            UserEmail = user.Email,
            AgencyId = user.AgencyId,
            AgencyName = user.Agency?.Name ?? string.Empty,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            PeriodDays = periodDays,
            TotalHoursMs = totalHoursMs,
            TotalEntries = entries.Count,
            WorkingDaysCount = workingDays,
            DaysWithoutEntries = periodDays - workingDays,
            AverageHoursPerDayMs = periodDays > 0 ? totalHoursMs / periodDays : 0,
            AverageHoursPerWorkingDayMs = workingDays > 0 ? totalHoursMs / workingDays : 0,

            // Daily Breakdown
            DailyBreakdown = entries
                .GroupBy(e => e.EntryDate.Date)
                .Select(g => new DailyBreakdownDto
                {
                    Date = g.Key,
                    HoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Clients = g.Select(e => e.Client.Name).Distinct().ToList(),
                    Projects = g.Select(e => e.ProjectBrand).Distinct().ToList()
                })
                .OrderBy(d => d.Date)
                .ToList(),

            // Client Breakdown
            ClientBreakdown = entries
                .GroupBy(e => new { e.ClientId, e.Client.Name })
                .Select(g => new ClientBreakdownDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHoursMs > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHoursMs * 100, 2)
                        : 0,
                    Projects = g.Select(e => e.ProjectBrand).Distinct().ToList()
                })
                .OrderByDescending(c => c.TotalHoursMs)
                .ToList(),

            // Job Type Breakdown
            JobTypeBreakdown = entries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new JobTypeBreakdownDto
                {
                    JobTypeId = g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHoursMs > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHoursMs * 100, 2)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .ToList(),

            // Project Breakdown
            ProjectBreakdown = entries
                .GroupBy(e => new
                {
                    ProjectBrandName = e.ProjectBrand,
                    ClientName = e.Client.Name
                })
                .Select(g => new ProjectBreakdownDto
                {
                    ProjectBrandId = 0, // больше нет ID
                    ProjectBrandName = g.Key.ProjectBrandName,
                    ClientName = g.Key.ClientName,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHoursMs > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHoursMs * 100, 2)
                        : 0
                })
                .OrderByDescending(p => p.TotalHoursMs)
                .ToList()
        };

        // Кешируем результат
        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(CacheDurationMinutes));

        return report;
    }

    public async Task<TeamLoadReportDto> GetTeamLoadReportAsync(
        long agencyId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        // Проверка прав доступа
        if (!await CanUserAccessReportAsync(requestingUserId))
        {
            throw new UnauthorizedAccessException("Вы не имеете доступа к отчетам команды");
        }

        // Проверка кеша
        var cacheKey = $"{CacheKeyPrefix}team-load:{agencyId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out TeamLoadReportDto? cachedReport) && cachedReport != null)
        {
            return cachedReport;
        }

        // Получаем агентство
        var agency = await _unitOfWork.Agencies.GetByIdAsync(agencyId);
        if (agency == null)
        {
            throw new KeyNotFoundException($"Агентство с ID {agencyId} не найдено");
        }

        // Получаем всех пользователей агентства
        var users = await _userRepository
            .GetQueryable()
            .Where(u => u.AgencyId == agencyId)
            .AsNoTracking()
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();

        // Получаем все записи времени команды за период
        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .Include(te => te.Client)
            .Include(te => te.JobType)
            .Where(te => userIds.Contains(te.UserId) &&
                         te.EntryDate >= fromDate.Date &&
                         te.EntryDate <= toDate.Date)
            .AsNoTracking()
            .ToListAsync();

        var totalTeamHours = entries.Sum(e => e.HoursMilliseconds);
        var activeMembers = entries.Select(e => e.UserId).Distinct().Count();

        // Формируем отчет
        var report = new TeamLoadReportDto
        {
            AgencyId = agencyId,
            AgencyName = agency.Name,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            TotalTeamHoursMs = totalTeamHours,
            TotalMembers = users.Count,
            ActiveMembers = activeMembers,
            AverageHoursPerMemberMs = users.Count > 0 ? totalTeamHours / users.Count : 0,

            // Members Load
            MembersLoad = entries
                .GroupBy(e => new { e.UserId, e.User.Name, e.User.Email })
                .Select(g =>
                {
                    var userHours = g.Sum(e => e.HoursMilliseconds);
                    var workingDays = g.Select(e => e.EntryDate.Date).Distinct().Count();

                    return new UserLoadSummaryDto
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.Name,
                        UserEmail = g.Key.Email,
                        TotalHoursMs = userHours,
                        EntriesCount = g.Count(),
                        WorkingDays = workingDays,
                        AverageHoursPerDayMs = workingDays > 0 ? userHours / workingDays : 0,
                        LoadPercentage = totalTeamHours > 0
                            ? Math.Round((double)userHours / totalTeamHours * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(u => u.TotalHoursMs)
                .ToList(),

            // Top Clients
            TopClients = entries
                .GroupBy(e => new { e.ClientId, e.Client.Name })
                .Select(g => new ClientBreakdownDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalTeamHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalTeamHours * 100, 2)
                        : 0,
                    Users = g.Select(e => e.User.Name).Distinct().ToList()
                })
                .OrderByDescending(c => c.TotalHoursMs)
                .Take(10)
                .ToList(),

            // Job Type Distribution
            JobTypeDistribution = entries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new JobTypeBreakdownDto
                {
                    JobTypeId = g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalTeamHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalTeamHours * 100, 2)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .ToList()
        };

        // Кешируем
        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(CacheDurationMinutes));

        return report;
    }

    public async Task<ClientReportDto> GetClientReportAsync(
        long clientId,
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        // Проверка прав доступа
        if (!await CanUserAccessReportAsync(requestingUserId))
        {
            throw new UnauthorizedAccessException("Вы не имеете доступа к отчетам по клиентам");
        }

        // Проверка кеша
        var cacheKey = $"{CacheKeyPrefix}client:{clientId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out ClientReportDto? cachedReport) && cachedReport != null)
        {
            return cachedReport;
        }

        // Получаем клиента
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new KeyNotFoundException($"Клиент с ID {clientId} не найден");
        }

        // Получаем записи времени по клиенту
        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .ThenInclude(u => u.Agency)
            .Include(te => te.JobType)
            .Include(te => te.Media)
            .Where(te => te.ClientId == clientId &&
                         te.EntryDate >= fromDate.Date &&
                         te.EntryDate <= toDate.Date)
            .AsNoTracking()
            .ToListAsync();

        var totalHours = entries.Sum(e => e.HoursMilliseconds);

        var report = new ClientReportDto
        {
            ClientId = clientId,
            ClientName = client.Name,
            ClientEmail = client.Email,
            ClientPhone = client.Phone,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            TotalHoursMs = totalHours,
            TotalEntries = entries.Count,
            UniqueUsers = entries.Select(e => e.UserId).Distinct().Count(),
            UniqueProjects = entries.Select(e => e.ProjectBrand).Distinct().Count(),

            // User Contributions
            UserContributions = entries
                .GroupBy(e => new { e.UserId, UserName = e.User.Name, AgencyName = e.User.Agency.Name })
                .Select(g => new UserContributionDto
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    AgencyName = g.Key.AgencyName,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    ContributionPercentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(u => u.TotalHoursMs)
                .ToList(),

            // Project Breakdown
            ProjectBreakdown = entries
                .GroupBy(e => e.ProjectBrand)
                .Select(g => new ProjectBreakdownDto
                {
                    ProjectBrandId = 0,
                    ProjectBrandName = g.Key,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0,
                    ClientName = client.Name
                })
                .OrderByDescending(p => p.TotalHoursMs)
                .ToList(),

            // Job Type Breakdown
            JobTypeBreakdown = entries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new JobTypeBreakdownDto
                {
                    JobTypeId = g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .ToList(),

            // Media Breakdown
            MediaBreakdown = entries
                .GroupBy(e => new { e.MediaId, e.Media.Name })
                .Select(g => new MediaBreakdownDto
                {
                    MediaId = g.Key.MediaId,
                    MediaName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(m => m.TotalHoursMs)
                .ToList(),

            // Daily Breakdown
            DailyBreakdown = entries
                .GroupBy(e => e.EntryDate.Date)
                .Select(g => new DailyBreakdownDto
                {
                    Date = g.Key,
                    HoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Projects = g.Select(e => e.ProjectBrand).Distinct().ToList()
                })
                .OrderBy(d => d.Date)
                .ToList()
        };

        // Кешируем
        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(CacheDurationMinutes));

        return report;
    }

    public async Task<IEnumerable<ClientSummaryDto>> GetTopClientsReportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        long? agencyId = null,
        int top = 10)
    {
        // Проверка прав доступа
        if (!await CanUserAccessReportAsync(requestingUserId))
        {
            throw new UnauthorizedAccessException("Вы не имеете доступа к этому отчету");
        }

        // Базовый запрос
        var query = _timeEntryRepository
            .GetQueryable()
            .Include(te => te.Client)
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date);

        // Фильтр по агентству если указано
        if (agencyId.HasValue)
        {
            query = query.Where(te => te.AgencyId == agencyId.Value);
        }

        var entries = await query.AsNoTracking().ToListAsync();
        var totalHours = entries.Sum(e => e.HoursMilliseconds);

        var report = entries
            .GroupBy(e => new { e.ClientId, e.Client.Name })
            .Select(g => new ClientSummaryDto
            {
                ClientId = g.Key.ClientId,
                ClientName = g.Key.Name,
                TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                ProjectsCount = g.Select(e => e.ProjectBrand).Distinct().Count(),
                UsersCount = g.Select(e => e.UserId).Distinct().Count(),
                Percentage = totalHours > 0
                    ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                    : 0
            })
            .OrderByDescending(c => c.TotalHoursMs)
            .Take(top)
            .ToList();

        return report;
    }

    public async Task<IEnumerable<InactiveUserDto>> GetInactiveUsersThisWeekAsync(long requestingUserId)
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = (int)today.DayOfWeek;
        var monday = today.AddDays(dayOfWeek == 0 ? -6 : -(dayOfWeek - 1));
        var sunday = monday.AddDays(6);

        var activeUsers = await _userRepository.GetActiveUsersAsync();
        var usersWithEntries = await _timeEntryRepository.GetUserIdsWithEntriesAsync(monday, sunday);
        var withEntriesSet = usersWithEntries.ToHashSet();

        var inactiveUsers = activeUsers
            .Where(u => !withEntriesSet.Contains(u.Id))
            .ToList();

        var inactiveIds = inactiveUsers.Select(u => u.Id).ToList();
        var lastDates = await _timeEntryRepository.GetLastEntryDatesAsync(inactiveIds);

        return inactiveUsers
            .Select(u =>
            {
                var dto = _mapper.Map<InactiveUserDto>(u);
                dto.LastEntryDate = lastDates.GetValueOrDefault(u.Id);
                return dto;
            })
            .OrderBy(u => u.LastEntryDate ?? DateTime.MinValue);
    }

    public async Task<ProjectReportDto> GetProjectReportAsync(
        string projectBrandName, // было: long projectBrandId
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId)
    {
        if (!await CanUserAccessReportAsync(requestingUserId))
            throw new UnauthorizedAccessException("Вы не имеете доступа к отчетам по проектам");

        var cacheKey = $"{CacheKeyPrefix}project:{projectBrandName}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out ProjectReportDto? cachedReport) && cachedReport != null)
            return cachedReport;

        // Получаем записи по текстовому названию проекта
        var entries = await _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User).ThenInclude(u => u.Agency)
            .Include(te => te.Client)
            .Include(te => te.JobType)
            .Include(te => te.Media)
            .Where(te => te.ProjectBrand == projectBrandName &&
                         te.EntryDate >= fromDate.Date &&
                         te.EntryDate <= toDate.Date)
            .AsNoTracking()
            .ToListAsync();

        var totalHours = entries.Sum(e => e.HoursMilliseconds);

        var report = new ProjectReportDto
        {
            ProjectBrandId = 0,
            ProjectBrandName = projectBrandName,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            TotalHoursMs = totalHours,
            TotalEntries = entries.Count,
            UniqueUsers = entries.Select(e => e.UserId).Distinct().Count(),
            Clients = entries.Select(e => e.Client.Name).Distinct().ToList(),
            Agencies = entries.Select(e => e.User.Agency.Name).Distinct().ToList(),

            UserContributions = entries
                .GroupBy(e => new { e.UserId, UserName = e.User.Name, AgencyName = e.User.Agency.Name })
                .Select(g => new UserContributionDto
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    AgencyName = g.Key.AgencyName,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    ContributionPercentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(u => u.TotalHoursMs)
                .ToList(),

            JobTypeBreakdown = entries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new JobTypeBreakdownDto
                {
                    JobTypeId = g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .ToList(),

            MediaBreakdown = entries
                .GroupBy(e => new { e.MediaId, e.Media.Name })
                .Select(g => new MediaBreakdownDto
                {
                    MediaId = g.Key.MediaId,
                    MediaName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(m => m.TotalHoursMs)
                .ToList(),

            WeeklyBreakdown = entries
                .GroupBy(e => e.EntryDate.AddDays(-(int)e.EntryDate.DayOfWeek + (int)DayOfWeek.Monday))
                .Select(g =>
                {
                    var weekHours = g.Sum(e => e.HoursMilliseconds);
                    var workingDays = g.Select(e => e.EntryDate.Date).Distinct().Count();
                    return new WeeklyBreakdownDto
                    {
                        WeekStart = g.Key,
                        WeekEnd = g.Key.AddDays(6),
                        HoursMs = weekHours,
                        EntriesCount = g.Count(),
                        WorkingDays = workingDays,
                        AverageHoursPerDayMs = workingDays > 0 ? weekHours / workingDays : 0
                    };
                })
                .OrderBy(w => w.WeekStart)
                .ToList()
        };

        for (int i = 0; i < report.WeeklyBreakdown.Count; i++)
            report.WeeklyBreakdown[i].WeekNumber = i + 1;

        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(CacheDurationMinutes));

        return report;
    }

    public async Task<TimeSummaryReportDto> GetTimeSummaryReportAsync(
        DateTime fromDate,
        DateTime toDate,
        long requestingUserId,
        long? agencyId = null,
        long? clientId = null)
    {
        // Проверка прав доступа
        if (!await CanUserAccessReportAsync(requestingUserId))
        {
            throw new UnauthorizedAccessException("Вы не имеете доступа к сводным отчетам");
        }

        // Проверка кеша
        var cacheKey = $"{CacheKeyPrefix}summary:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}:{agencyId}:{clientId}";
        if (_cache.TryGetValue(cacheKey, out TimeSummaryReportDto? cachedReport) && cachedReport != null)
        {
            return cachedReport;
        }

        // Базовый запрос с параллельной загрузкой
        var query = _timeEntryRepository
            .GetQueryable()
            .Include(te => te.User)
            .ThenInclude(u => u.Agency)
            .Include(te => te.Client)
            .Include(te => te.JobType)
            .Include(te => te.Media)
            .Where(te => te.EntryDate >= fromDate.Date && te.EntryDate <= toDate.Date);

        // Применяем фильтры
        if (agencyId.HasValue)
        {
            query = query.Where(te => te.AgencyId == agencyId.Value);
        }

        if (clientId.HasValue)
        {
            query = query.Where(te => te.ClientId == clientId.Value);
        }

        var entries = await query.AsNoTracking().ToListAsync();

        var totalHours = entries.Sum(e => e.HoursMilliseconds);

        // Получаем названия для фильтров
        string? agencyName = null;
        string? clientName = null;

        if (agencyId.HasValue)
        {
            var agency = await _unitOfWork.Agencies.GetByIdAsync(agencyId.Value);
            agencyName = agency?.Name;
        }

        if (clientId.HasValue)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId.Value);
            clientName = client?.Name;
        }

        var report = new TimeSummaryReportDto
        {
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            AgencyId = agencyId,
            AgencyName = agencyName,
            ClientId = clientId,
            ClientName = clientName,
            TotalHoursMs = totalHours,
            TotalEntries = entries.Count,
            TotalUsers = entries.Select(e => e.UserId).Distinct().Count(),
            TotalClients = entries.Select(e => e.ClientId).Distinct().Count(),
            TotalProjects = entries.Select(e => e.ProjectBrand).Distinct().Count(),

            // Top Users
            TopUsers = entries
                .GroupBy(e => new { e.UserId, e.User.Name, e.User.Email })
                .Select(g =>
                {
                    var userHours = g.Sum(e => e.HoursMilliseconds);
                    var workingDays = g.Select(e => e.EntryDate.Date).Distinct().Count();

                    return new UserLoadSummaryDto
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.Name,
                        UserEmail = g.Key.Email,
                        TotalHoursMs = userHours,
                        EntriesCount = g.Count(),
                        WorkingDays = workingDays,
                        AverageHoursPerDayMs = workingDays > 0 ? userHours / workingDays : 0,
                        LoadPercentage = totalHours > 0
                            ? Math.Round((double)userHours / totalHours * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(u => u.TotalHoursMs)
                .Take(10)
                .ToList(),

            // Top Clients
            TopClients = entries
                .GroupBy(e => new { e.ClientId, e.Client.Name })
                .Select(g => new ClientSummaryDto
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    ProjectsCount = g.Select(e => e.ProjectBrand).Distinct().Count(),
                    UsersCount = g.Select(e => e.UserId).Distinct().Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(c => c.TotalHoursMs)
                .Take(10)
                .ToList(),

            // Top Projects
            TopProjects = entries
                .GroupBy(e => new { ProjectBrandName = e.ProjectBrand, ClientName = e.Client.Name })
                .Select(g => new ProjectSummaryDto
                {
                    ProjectBrandId = 0,
                    ProjectBrandName = g.Key.ProjectBrandName,
                    ClientName = g.Key.ClientName,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    UsersCount = g.Select(e => e.UserId).Distinct().Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(p => p.TotalHoursMs)
                .Take(10)
                .ToList(),

            // Agency Breakdown
            AgencyBreakdown = entries
                .GroupBy(e => new { e.User.AgencyId, e.User.Agency.Name })
                .Select(g => new AgencyBreakdownDto
                {
                    AgencyId = g.Key.AgencyId,
                    AgencyName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    TotalUsers = g.Select(e => e.UserId).Distinct().Count(),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(a => a.TotalHoursMs)
                .ToList(),

            // Job Type Breakdown
            JobTypeBreakdown = entries
                .GroupBy(e => new { e.JobTypeId, e.JobType.Name })
                .Select(g => new JobTypeBreakdownDto
                {
                    JobTypeId = g.Key.JobTypeId,
                    JobTypeName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(j => j.TotalHoursMs)
                .ToList(),

            // Media Breakdown
            MediaBreakdown = entries
                .GroupBy(e => new { e.MediaId, e.Media.Name })
                .Select(g => new MediaBreakdownDto
                {
                    MediaId = g.Key.MediaId,
                    MediaName = g.Key.Name,
                    TotalHoursMs = g.Sum(e => e.HoursMilliseconds),
                    EntriesCount = g.Count(),
                    Percentage = totalHours > 0
                        ? Math.Round((double)g.Sum(e => e.HoursMilliseconds) / totalHours * 100, 2)
                        : 0
                })
                .OrderByDescending(m => m.TotalHoursMs)
                .ToList(),

            // Weekly Trends
            WeeklyTrends = entries
                .GroupBy(e =>
                {
                    var weekStart = e.EntryDate.AddDays(-(int)e.EntryDate.DayOfWeek + (int)DayOfWeek.Monday);
                    return weekStart;
                })
                .Select(g =>
                {
                    var weekHours = g.Sum(e => e.HoursMilliseconds);
                    var workingDays = g.Select(e => e.EntryDate.Date).Distinct().Count();

                    return new WeeklyBreakdownDto
                    {
                        WeekStart = g.Key,
                        WeekEnd = g.Key.AddDays(6),
                        HoursMs = weekHours,
                        EntriesCount = g.Count(),
                        WorkingDays = workingDays,
                        AverageHoursPerDayMs = workingDays > 0 ? weekHours / workingDays : 0
                    };
                })
                .OrderBy(w => w.WeekStart)
                .ToList()
        };

        // Вычисляем номера недель
        for (int i = 0; i < report.WeeklyTrends.Count; i++)
        {
            report.WeeklyTrends[i].WeekNumber = i + 1;
        }

        // Кешируем
        _cache.Set(cacheKey, report, TimeSpan.FromMinutes(CacheDurationMinutes));

        return report;
    }

    public async Task<bool> CanUserAccessReportAsync(long requestingUserId, long? targetUserId = null)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(requestingUserId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        var roles = user.UserRoles
            .Where(ur => ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToList();

        // Admin и Manager могут видеть все отчеты
        if (roles.Contains("Admin") || roles.Contains("Manager"))
        {
            return true;
        }

        // Обычный сотрудник может видеть только свои отчеты
        if (targetUserId.HasValue)
        {
            return targetUserId.Value == requestingUserId;
        }

        // Если targetUserId не указан - запрещаем доступ обычным сотрудникам
        return false;
    }
}