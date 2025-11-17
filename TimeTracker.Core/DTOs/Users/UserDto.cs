namespace TimeTracker.Core.DTOs.Users;

public class UserDto
{
    public long Id { get; set; }
    
    public string Login { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public long AgencyId { get; set; }
    
    public string AgencyName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}