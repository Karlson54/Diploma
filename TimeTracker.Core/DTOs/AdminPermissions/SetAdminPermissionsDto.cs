using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.AdminPermissions;

public class SetAdminPermissionsDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Потрібно вказати хоча б один дозвіл")]
    public List<AdminPermissionItemDto> Permissions { get; set; } = new();
}

public class AdminPermissionItemDto
{
    [Required]
    [Range(1, long.MaxValue)]
    public long AgencyId { get; set; }

    [Required]
    [Range(1, long.MaxValue)]
    public long DepartmentId { get; set; }
}