using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NotFutureDateAttribute : ValidationAttribute
{
    public NotFutureDateAttribute()
    {
        ErrorMessage = ValidationConstants.NotFutureDateError;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is DateTime dateValue)
        {
            // Дозволяємо тільки сьогодні (за київським часом)
            var maxAllowedDate = TimeZoneHelper.TodayInKyiv();

            if (dateValue.Date > maxAllowedDate)
            {
                return new ValidationResult(
                    ErrorMessage ?? ValidationConstants.NotFutureDateError,
                    new[] { validationContext.MemberName ?? "EntryDate" });
            }
        }

        return ValidationResult.Success;
    }
}