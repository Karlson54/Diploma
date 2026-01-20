using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Validators;

namespace TimeTracker.Core.DTOs.TimeEntries;

public class CopyDayDto
{
    public long? UserId { get; set; }

    [Required(ErrorMessage = "Вихідна дата обов'язкова")]
    [DataType(DataType.Date)]
    public DateTime SourceDate { get; set; }

    [Required(ErrorMessage = "Цільова дата обов'язкова")]
    [DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime TargetDate { get; set; }
}