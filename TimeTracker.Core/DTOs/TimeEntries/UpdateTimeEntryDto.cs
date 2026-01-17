using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;
using TimeTracker.Core.Validators;

namespace TimeTracker.Core.DTOs.TimeEntries;

public class UpdateTimeEntryDto : IValidatableObject
{
    [Required(ErrorMessage = "Дата запису обов'язкова")]
    [DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime EntryDate { get; set; }

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }

    [Required(ErrorMessage = "MarketId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "MarketId має бути додатним числом")]
    public long MarketId { get; set; }

    [Required(ErrorMessage = "ContractingAgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ContractingAgencyId має бути додатним числом")]
    public long ContractingAgencyId { get; set; }

    [Required(ErrorMessage = "ClientId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ClientId має бути додатним числом")]
    public long ClientId { get; set; }

    [Required(ErrorMessage = "ProjectBrandId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ProjectBrandId має бути додатним числом")]
    public long ProjectBrandId { get; set; }

    [Required(ErrorMessage = "MediaId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "MediaId має бути додатним числом")]
    public long MediaId { get; set; }

    [Required(ErrorMessage = "JobTypeId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "JobTypeId має бути додатним числом")]
    public long JobTypeId { get; set; }

    [Required(ErrorMessage = "Кількість годин обов'язкова")]
    [Range(ValidationConstants.MinHoursMs, ValidationConstants.MaxHoursPerDayMs, 
        ErrorMessage = "Час має бути від 1 мілісекунди до 24 годин (86400000 мс)")]
    [MaxDailyHours]
    public long HoursMilliseconds { get; set; }

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
        
        // Перевірка що EntryDate не дуже давня
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);
        if (EntryDate < oneYearAgo)
        {
            yield return new ValidationResult(
                "Дата запису не може бути більше року назад",
                new[] { nameof(EntryDate) });
        }
    }
}