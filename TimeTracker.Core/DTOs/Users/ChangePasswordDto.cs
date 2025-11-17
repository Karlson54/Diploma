using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Users;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Поточний пароль обов'язковий")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Поточний пароль має бути від 6 до 100 символів")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новий пароль обов'язковий")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Новий пароль має бути від 6 до 100 символів")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", 
        ErrorMessage = "Новий пароль має містити хоча б одну велику літеру, одну малу літеру та одну цифру")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(NewPassword), ErrorMessage = "Новий пароль та підтвердження не збігаються")]
    public string ConfirmPassword { get; set; } = string.Empty;
}