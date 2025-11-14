namespace TimeTracker.Core.DTOs.Roles;

public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Permissions { get; set; }
    public bool IsActive { get; set; }
}