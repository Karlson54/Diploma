namespace TimeTracker.Core.DTOs.Users;

public class CreateUserDto
{
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long AgencyId { get; set; }
    public List<long> RoleIds { get; set; } = new();
}