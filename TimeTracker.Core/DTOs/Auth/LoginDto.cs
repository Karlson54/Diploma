using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Login або Email обов'язковий")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Login/Email має бути від 3 до 100 символів")]
    public string LoginOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має бути від 6 до 100 символів")]
    public string Password { get; set; } = string.Empty;
}