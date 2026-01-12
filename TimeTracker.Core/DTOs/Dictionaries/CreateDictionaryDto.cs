using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Dictionaries;

public class CreateDictionaryDto
{
    [Required(ErrorMessage = "Назва обов'язкова")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має бути від 2 до 100 символів")]
    public string Name { get; set; } = string.Empty;
}