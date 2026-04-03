using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.Common;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.TimeTracking;

public class TimeValidationService : ITimeValidationService
{
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimeValidationService> _logger;

    public TimeValidationService(
        ITimeEntryRepository timeEntryRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<TimeValidationService> logger)
    {
        _timeEntryRepository = timeEntryRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateCreateAsync(
        long userId,
        DateTime entryDate,
        long hoursMilliseconds)
    {
        var result = new ValidationResult { IsValid = true };

        // 1. Базова валідація часу
        if (!TimeHelper.IsValidDailyHours(hoursMilliseconds))
        {
            result.AddError(ValidationConstants.MaxDailyHoursError);
            return result;
        }

        // 2. Перевірка що дата не в майбутньому
        var maxAllowedDate = DateTime.UtcNow.Date.AddDays(1);
        if (entryDate.Date > maxAllowedDate)
        {
            result.AddError(ValidationConstants.NotFutureDateError);
        }

        // 3. Перевірка що користувач існує і активний
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            result.AddError($"Користувача з ID {userId} не знайдено");
            return result;
        }

        if (!user.IsActive)
        {
            result.AddError("Користувач деактивований");
            return result;
        }

        // 4. Перевірка загальної кількості годин за день
        var totalHoursForDay = await GetTotalHoursForDayAsync(userId, entryDate);
        var totalWithNew = totalHoursForDay + hoursMilliseconds;

        if (totalWithNew > ValidationConstants.MaxHoursPerDayMs)
        {
            var existingHours = TimeHelper.FormatHours(totalHoursForDay);
            var newHours = TimeHelper.FormatHours(hoursMilliseconds);
            var totalHours = TimeHelper.FormatHours(totalWithNew);

            result.AddError(
                $"Перевищено ліміт часу за день. " +
                $"Вже зареєстровано: {existingHours}, додається: {newHours}, " +
                $"загалом буде: {totalHours} (максимум 24:00)");
        }

        return result;
    }

    public async Task<ValidationResult> ValidateUpdateAsync(
        long entryId,
        long userId,
        DateTime entryDate,
        long hoursMilliseconds)
    {
        var result = new ValidationResult { IsValid = true };

        // 1. Перевірка що запис існує
        var existingEntry = await _timeEntryRepository.GetByIdAsync(entryId);
        if (existingEntry == null)
        {
            result.AddError($"TimeEntry з ID {entryId} не знайдено");
            return result;
        }

        // 2. Перевірка що запис належить користувачу
        if (existingEntry.UserId != userId)
        {
            result.AddError("Ви не можете редагувати чужий запис часу");
            return result;
        }

        // 3. Базова валідація часу
        if (!TimeHelper.IsValidDailyHours(hoursMilliseconds))
        {
            result.AddError(ValidationConstants.MaxDailyHoursError);
            return result;
        }

        // 4. Перевірка що дата не в майбутньому
        var maxAllowedDate = DateTime.UtcNow.Date.AddDays(1);
        if (entryDate.Date > maxAllowedDate)
        {
            result.AddError(ValidationConstants.NotFutureDateError);
        }

        // 5. Перевірка загальної кількості годин за день (виключаючи поточний запис)
        var totalHoursForDay = await GetTotalHoursForDayAsync(userId, entryDate, entryId);
        var totalWithUpdated = totalHoursForDay + hoursMilliseconds;

        if (totalWithUpdated > ValidationConstants.MaxHoursPerDayMs)
        {
            var existingHours = TimeHelper.FormatHours(totalHoursForDay);
            var newHours = TimeHelper.FormatHours(hoursMilliseconds);
            var totalHours = TimeHelper.FormatHours(totalWithUpdated);

            result.AddError(
                $"Перевищено ліміт часу за день. " +
                $"Інші записи за день: {existingHours}, оновлюється на: {newHours}, " +
                $"загалом буде: {totalHours} (максимум 24:00)");
        }

        return result;
    }

    public async Task<ValidationResult> ValidateReferencesAsync(
        long agencyId,
        long marketId,
        long contractingAgencyId,
        long clientId,
        long mediaId,
        long jobTypeId)
    {
        var result = new ValidationResult { IsValid = true };

        var agency = await _unitOfWork.Agencies.GetByIdAsync(agencyId);
        var market = await _unitOfWork.Markets.GetByIdAsync(marketId);
        var contractingAgency = await _unitOfWork.ContractingAgencies.GetByIdAsync(contractingAgencyId);
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        var media = await _unitOfWork.Media.GetByIdAsync(mediaId);
        var jobType = await _unitOfWork.JobTypes.GetByIdAsync(jobTypeId);

        if (agency == null)
            result.AddError($"Agency з ID {agencyId} не знайдено");
        else if (!agency.IsActive)
            result.AddError($"Agency '{agency.Name}' деактивований");

        if (market == null)
            result.AddError($"Market з ID {marketId} не знайдено");
        else if (!market.IsActive)
            result.AddError($"Market '{market.Name}' деактивований");

        if (contractingAgency == null)
            result.AddError($"ContractingAgency з ID {contractingAgencyId} не знайдено");
        else if (!contractingAgency.IsActive)
            result.AddError($"ContractingAgency '{contractingAgency.Name}' деактивований");

        if (client == null)
            result.AddError($"Client з ID {clientId} не знайдено");
        else if (!client.IsActive)
            result.AddError($"Client '{client.Name}' деактивований");

        if (media == null)
            result.AddError($"Media з ID {mediaId} не знайдено");
        else if (!media.IsActive)
            result.AddError($"Media '{media.Name}' деактивований");

        if (jobType == null)
            result.AddError($"JobType з ID {jobTypeId} не знайдено");
        else if (!jobType.IsActive)
            result.AddError($"JobType '{jobType.Name}' деактивований");

        return result;
    }

    public async Task<ValidationResult> ValidateUserPermissionsAsync(
        long userId,
        long? targetUserId = null)
    {
        var result = new ValidationResult { IsValid = true };

        // Перевіряємо що користувач існує
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            result.AddError($"Користувача з ID {userId} не знайдено");
            return result;
        }

        if (!user.IsActive)
        {
            result.AddError("Ваш обліковий запис деактивований");
            return result;
        }

        // Якщо редагуємо чужий запис - потрібні права Manager або Admin
        if (targetUserId.HasValue && targetUserId.Value != userId)
        {
            var userWithRoles = await _userRepository.GetByIdWithRolesAsync(userId);
            var roles = userWithRoles?.UserRoles
                .Where(ur => ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToList() ?? new List<string>();

            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
            {
                result.AddError("Ви не маєте прав редагувати чужі записи часу");
            }
        }

        return result;
    }

    public async Task<long> GetTotalHoursForDayAsync(long userId, DateTime date)
    {
        return await _timeEntryRepository.GetTotalHoursForDateAsync(userId, date);
    }

    public async Task<long> GetTotalHoursForDayAsync(long userId, DateTime date, long excludeEntryId)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        var totalHours = await _timeEntryRepository
            .GetQueryable()
            .Where(te => te.UserId == userId &&
                         te.EntryDate >= startOfDay &&
                         te.EntryDate < endOfDay &&
                         te.Id != excludeEntryId)
            .SumAsync(te => te.HoursMilliseconds);

        return totalHours;
    }
}