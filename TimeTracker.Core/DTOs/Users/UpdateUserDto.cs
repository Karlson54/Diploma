using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Users;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(
        ValidationPatterns.EmailMaxLength, 
        ErrorMessage = "Email не може перевищувати {1} символів")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(
        ValidationPatterns.NameMaxLength, 
        MinimumLength = ValidationPatterns.NameMinLength, 
        ErrorMessage = "Name має бути від {2} до {1} символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }
}