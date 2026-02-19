using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Auth;

/// <summary>
/// Дані для реєстрації нового користувача
/// </summary>
public class RegisterDto
{
    /// <summary>Унікальний логін (тільки літери, цифри, підкреслення)</summary>
    [Required(ErrorMessage = "Login обов'язковий")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$")]
    public string Login { get; set; } = string.Empty;

    /// <summary>Унікальний email</summary>
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Пароль (мін. 8 символів, велика/мала літера, цифра, спецсимвол)</summary>
    [Required(ErrorMessage = "Password обов'язковий")]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Підтвердження пароля — має співпадати з Password</summary>
    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Повне ім'я користувача</summary>
    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>ID агентства до якого належить користувач</summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long AgencyId { get; set; }
}