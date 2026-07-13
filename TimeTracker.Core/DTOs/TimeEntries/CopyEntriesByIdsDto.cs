using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Validators;

namespace TimeTracker.Core.DTOs.TimeEntries;

public class CopyEntriesByIdsDto
{
    [Required(ErrorMessage = "Список ID записів обов'язковий")]
    [MinLength(1, ErrorMessage = "Потрібно вказати хоча б один запис для копіювання")]
    public List<long> EntryIds { get; set; } = new();

    [Required(ErrorMessage = "Цільова дата обов'язкова")]
    [DataType(DataType.Date)]
    [NotFutureDate]
    public DateTime TargetDate { get; set; }
}