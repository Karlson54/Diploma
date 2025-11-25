using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Auth;

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Поточний пароль обов'язковий")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новий пароль обов'язковий")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Новий пароль має бути від 8 до 100 символів")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:,.<>?/~`]).{8,}$", 
        ErrorMessage = "Новий пароль не відповідає вимогам безпеки")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}