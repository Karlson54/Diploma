using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Users;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name обов'язкове")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name має бути від 2 до 100 символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }

    public bool IsActive { get; set; }
}