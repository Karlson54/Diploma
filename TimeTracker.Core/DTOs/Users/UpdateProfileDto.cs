using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Users;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(
        ValidationPatterns.NameMaxLength,
        MinimumLength = ValidationPatterns.NameMinLength,
        ErrorMessage = "Name має бути від {2} до {1} символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(
        ValidationPatterns.EmailMaxLength,
        ErrorMessage = "Email не може перевищувати {1} символів")]
    public string Email { get; set; } = string.Empty;

    [StringLength(
        ValidationPatterns.LoginMaxLength,
        MinimumLength = ValidationPatterns.LoginMinLength,
        ErrorMessage = "Login має бути від {2} до {1} символів")]
    [RegularExpression(
        ValidationPatterns.Login,
        ErrorMessage = ValidationPatterns.LoginErrorMessage)]
    public string? Login { get; set; }
}