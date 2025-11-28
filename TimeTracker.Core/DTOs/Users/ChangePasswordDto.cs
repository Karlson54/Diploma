using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Users;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Поточний пароль обов'язковий")]
    [StringLength(
        ValidationPatterns.PasswordMaxLength, 
        MinimumLength = ValidationPatterns.PasswordMinLength, 
        ErrorMessage = "Поточний пароль має бути від {2} до {1} символів")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новий пароль обов'язковий")]
    [StringLength(
        ValidationPatterns.PasswordMaxLength, 
        MinimumLength = ValidationPatterns.PasswordMinLength, 
        ErrorMessage = "Новий пароль має бути від {2} до {1} символів")]
    [RegularExpression(
        ValidationPatterns.Password, 
        ErrorMessage = ValidationPatterns.PasswordErrorMessage)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(NewPassword), ErrorMessage = "Новий пароль та підтвердження не збігаються")]
    public string ConfirmPassword { get; set; } = string.Empty;
}