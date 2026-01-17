using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MaxDailyHoursAttribute : ValidationAttribute
{
    public MaxDailyHoursAttribute()
    {
        ErrorMessage = ValidationConstants.MaxDailyHoursError;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is long hoursMs)
        {
            if (!TimeHelper.IsValidDailyHours(hoursMs))
            {
                return new ValidationResult(
                    ErrorMessage ?? ValidationConstants.MaxDailyHoursError,
                    new[] { validationContext.MemberName ?? "HoursMilliseconds" });
            }
        }

        return ValidationResult.Success;
    }
}