namespace TimeTracker.Core.DTOs.Users;

public class UserListItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AgencyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RolesCount { get; set; }
}