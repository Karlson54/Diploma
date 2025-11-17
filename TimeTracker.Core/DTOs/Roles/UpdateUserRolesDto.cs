using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Roles;

public class UpdateUserRolesDto
{
    [Required(ErrorMessage = "Список ролей обов'язковий")]
    [MinLength(1, ErrorMessage = "Користувач повинен мати хоча б одну роль")]
    public List<long> RoleIds { get; set; } = new();
}