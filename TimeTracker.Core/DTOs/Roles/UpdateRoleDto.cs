using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Roles;

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Назва ролі обов'язкова")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Назва ролі має бути від 3 до 50 символів")]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$", 
        ErrorMessage = "Назва повинна починатися з літери та містити тільки літери, цифри та підкреслення")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
    public string? Description { get; set; }

    [StringLength(4000, ErrorMessage = "Permissions JSON не може перевищувати 4000 символів")]
    public string? Permissions { get; set; }
}