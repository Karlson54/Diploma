using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;
using TimeTracker.Core.Validators;

namespace TimeTracker.Core.DTOs.TimeEntries;

/// <summary>
/// Дані для створення нового запису робочого часу
/// </summary>
public class CreateTimeEntryDto : IValidatableObject
{
    /// <summary>
    /// ID співробітника для якого створюється запис
    /// </summary>
    [Required(ErrorMessage = "UserId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "UserId має бути додатним числом")]
    public long UserId { get; set; }

    /// <summary>
    /// Дата запису (не може бути в майбутньому)
    /// </summary>
    [Required(ErrorMessage = "Дата запису обов'язкова")]
    [DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// ID агентства
    /// </summary>
    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }

    /// <summary>
    /// ID ринку
    /// </summary>
    [Required(ErrorMessage = "MarketId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "MarketId має бути додатним числом")]
    public long MarketId { get; set; }

    /// <summary>
    /// ID ринку
    /// </summary>
    [Required(ErrorMessage = "ContractingAgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ContractingAgencyId має бути додатним числом")]
    public long ContractingAgencyId { get; set; }

    /// <summary>
    /// ID клієнта
    /// </summary>
    [Required(ErrorMessage = "ClientId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ClientId має бути додатним числом")]
    public long ClientId { get; set; }

    /// <summary>
    /// ID проекту/бренду
    /// </summary>
    [Required(ErrorMessage = "ProjectBrandId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ProjectBrandId має бути додатним числом")]
    public long ProjectBrandId { get; set; }

    /// <summary>
    /// ID медіаканалу
    /// </summary>
    [Required(ErrorMessage = "MediaId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "MediaId має бути додатним числом")]
    public long MediaId { get; set; }

    /// <summary>
    /// ID типу роботи
    /// </summary>
    [Required(ErrorMessage = "JobTypeId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "JobTypeId має бути додатним числом")]
    public long JobTypeId { get; set; }

    /// <summary>
    /// Витрачений час у мілісекундах.
    /// Мінімум: 1 мс. Максимум: 86 400 000 мс (24 години).
    /// </summary>
    [Required(ErrorMessage = "Кількість годин обов'язкова")]
    [Range(ValidationConstants.MinHoursMs, ValidationConstants.MaxHoursPerDayMs,
        ErrorMessage = "Час має бути від 1 мілісекунди до 24 годин (86400000 мс)")]
    [MaxDailyHours]
    public long HoursMilliseconds { get; set; }

    /// <summary>
    /// Коментар до запису (необов'язковий)
    /// </summary>
    [StringLength(ValidationConstants.MaxCommentsLength,
        ErrorMessage = "Коментар не може перевищувати {1} символів")]
    public string? Comments { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Перевірка валідності часу
        if (HoursMilliseconds > 0 && !TimeHelper.IsValidDailyHours(HoursMilliseconds))
        {
            yield return new ValidationResult(
                ValidationConstants.MaxDailyHoursError,
                new[] { nameof(HoursMilliseconds) });
        }

        // Перевірка що EntryDate не дуже давня (наприклад, не більше року назад)
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        if (EntryDate < oneYearAgo)
        {
            yield return new ValidationResult(
                "Дата запису не може бути більше року назад",
                new[] { nameof(EntryDate) });
        }
    }
}