namespace TimeTracker.Core.DTOs.Roles;

public class RoleListItemDto
{
    public long Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
    
    public int UsersCount { get; set; }
}