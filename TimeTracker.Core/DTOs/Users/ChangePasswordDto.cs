using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Users;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Поточний пароль обов'язковий")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новий пароль обов'язковий")]
    [RegularExpression(
        ValidationPatterns.Password,
        ErrorMessage = ValidationPatterns.PasswordErrorMessage)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(NewPassword), ErrorMessage = "Новий пароль та підтвердження не збігаються")]
    public string ConfirmPassword { get; set; } = string.Empty;
}