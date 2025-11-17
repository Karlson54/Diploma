using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Roles;

public class AssignRoleDto
{
    [Required(ErrorMessage = "UserId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "UserId має бути додатним числом")]
    public long UserId { get; set; }

    [Required(ErrorMessage = "RoleId обов'язковий")]
    [Range(1, long.MaxValue, ErrorMessage = "RoleId має бути додатним числом")]
    public long RoleId { get; set; }
}