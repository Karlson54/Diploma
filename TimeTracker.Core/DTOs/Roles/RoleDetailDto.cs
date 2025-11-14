namespace TimeTracker.Core.DTOs.Roles;

public class RoleDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Permissions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UsersCount { get; set; }
    public List<string> PermissionsList { get; set; } = new();
}