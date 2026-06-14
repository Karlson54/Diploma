using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Departments;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Назва відділу обов'язкова")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має бути від 2 до 100 символів")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "AgencyId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "AgencyId має бути додатним числом")]
    public long AgencyId { get; set; }
}