using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Dictionaries.Agencies;

public class UpdateAgencyDto : UpdateDictionaryDto
{
    [Required(ErrorMessage = "Країна обов'язкова")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва країни має бути від 2 до 100 символів")]
    public string Country { get; set; } = "Ukraine";
}