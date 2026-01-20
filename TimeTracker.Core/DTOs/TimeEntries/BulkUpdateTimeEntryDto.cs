using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.TimeEntries;

public class BulkUpdateTimeEntryDto
{
    [Required(ErrorMessage = "ID запису обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "ID має бути додатним числом")]
    public long Id { get; set; }

    [Required(ErrorMessage = "Дані для оновлення обов'язкові")]
    public UpdateTimeEntryDto Data { get; set; } = null!;
}