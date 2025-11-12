namespace TimeTracker.Core.DTOs.Roles;

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Permissions { get; set; }
}