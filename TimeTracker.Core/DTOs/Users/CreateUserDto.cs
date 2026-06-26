using System.ComponentModel.DataAnnotations;
using TimeTracker.Core.Common;

namespace TimeTracker.Core.DTOs.Users;

public class CreateUserDto
{
    [Required(ErrorMessage = "Login обов'язковий")]
    [StringLength(
        ValidationPatterns.LoginMaxLength,
        MinimumLength = ValidationPatterns.LoginMinLength,
        ErrorMessage = "Login має бути від {2} до {1} символів")]
    [RegularExpression(
        ValidationPatterns.Login,
        ErrorMessage = ValidationPatterns.LoginErrorMessage)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(
        ValidationPatterns.EmailMaxLength,
        ErrorMessage = "Email не може перевищувати {1} символів")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password обов'язковий")]
    [RegularExpression(
        ValidationPatterns.Password,
        ErrorMessage = ValidationPatterns.PasswordErrorMessage)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(
        ValidationPatterns.NameMaxLength,
        MinimumLength = ValidationPatterns.NameMinLength,
        ErrorMessage = "Name має бути від {2} до {1} символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }

    [MaxLength(ValidationPatterns.MaxUserRolesCount, ErrorMessage = "Максимальна кількість ролей: {1}")]
    public List<long> RoleId { get; set; } = new();
    
    [Required(ErrorMessage = "DepartmentId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "DepartmentId має бути додатним числом")]
    public long DepartmentId { get; set; }
}