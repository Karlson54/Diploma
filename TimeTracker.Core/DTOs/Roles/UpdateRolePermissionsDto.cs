namespace TimeTracker.Core.DTOs.Roles;

public class UpdateRolePermissionsDto
{
    public List<string> Permissions { get; set; } = new();
}