using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Login обов'язковий")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Login має бути від 3 до 50 символів")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", 
        ErrorMessage = "Login може містити тільки літери, цифри та підкреслення")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password обов'язковий")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password має бути від 8 до 100 символів")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;:,.<>?/~`]).{8,}$", 
        ErrorMessage = "Password має містити велику літеру, малу літеру, цифру та спеціальний символ")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name має бути від 2 до 100 символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }
}