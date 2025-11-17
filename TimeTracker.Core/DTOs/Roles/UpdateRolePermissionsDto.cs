using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Roles;

public class UpdateRolePermissionsDto
{
    [Required(ErrorMessage = "Список permissions обов'язковий")]
    public List<string> Permissions { get; set; } = new();
}