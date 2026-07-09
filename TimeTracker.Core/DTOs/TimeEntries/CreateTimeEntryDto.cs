using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;
using TimeTracker.Core.Validators;

namespace TimeTracker.Core.DTOs.TimeEntries;

/// <summary>
/// Дані для створення нового запису робочого часу
/// </summary>
public class CreateTimeEntryDto : IValidatableObject
{
    [Required(ErrorMessage = "UserId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "UserId має бути додатним числом")]
    public long UserId { get; set; }

    [Required(ErrorMessage = "Дата запису обов'язкова")]
    [DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime EntryDate { get; set; }

    public long? MarketId { get; set; }

    public long? ContractingAgencyId { get; set; }

    public long? ClientId { get; set; }

    [StringLength(200, ErrorMessage = "ProjectBrand не може перевищувати 200 символів")]
    public string? ProjectBrand { get; set; }

    public long? MediaId { get; set; }

    public long? JobTypeId { get; set; }

    [Required(ErrorMessage = "Кількість годин обов'язкова")]
    [Range(ValidationConstants.MinHoursMs, ValidationConstants.MaxHoursPerDayMs,
        ErrorMessage = "Час має бути від 1 мілісекунди до 24 годин (86400000 мс)")]
    public long HoursMilliseconds { get; set; }

    [StringLength(ValidationConstants.MaxCommentsLength,
        ErrorMessage = "Коментар не може перевищувати {1} символів")]
    public string? Comments { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MarketId == null || MarketId <= 0)
            yield return new ValidationResult(
                "Оберіть Market зі списку",
                new[] { nameof(MarketId) });

        if (ContractingAgencyId == null || ContractingAgencyId <= 0)
            yield return new ValidationResult(
                "Оберіть Agency/Unit зі списку",
                new[] { nameof(ContractingAgencyId) });

        if (ClientId == null || ClientId <= 0)
            yield return new ValidationResult(
                "Оберіть Client зі списку",
                new[] { nameof(ClientId) });

        if (MediaId == null || MediaId <= 0)
            yield return new ValidationResult(
                "Оберіть Media зі списку",
                new[] { nameof(MediaId) });

        if (JobTypeId == null || JobTypeId <= 0)
            yield return new ValidationResult(
                "Оберіть JobType зі списку",
                new[] { nameof(JobTypeId) });

        if (HoursMilliseconds > 0 && !TimeHelper.IsValidDailyHours(HoursMilliseconds))
            yield return new ValidationResult(
                ValidationConstants.MaxDailyHoursError,
                new[] { nameof(HoursMilliseconds) });

        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        if (EntryDate < oneYearAgo)
            yield return new ValidationResult(
                "Дата запису не може бути більше року назад",
                new[] { nameof(EntryDate) });
    }
}