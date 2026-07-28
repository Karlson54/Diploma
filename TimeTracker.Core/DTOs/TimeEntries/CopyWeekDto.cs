using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.TimeEntries;

public class CopyWeekDto
{
    public long? UserId { get; set; }

    [Required(ErrorMessage = "Початок вихідного тижня обов'язковий")]
    [DataType(DataType.Date)]
    public DateTime SourceWeekStart { get; set; }

    [Required(ErrorMessage = "Початок цільового тижня обов'язковий")]
    [DataType(DataType.Date)]
    public DateTime TargetWeekStart { get; set; }
}