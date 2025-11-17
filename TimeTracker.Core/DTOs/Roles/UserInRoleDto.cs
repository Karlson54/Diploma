namespace TimeTracker.Core.DTOs.Roles;

public class UserInRoleDto
{
    public long Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string AgencyName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime AssignedAt { get; set; }
}
