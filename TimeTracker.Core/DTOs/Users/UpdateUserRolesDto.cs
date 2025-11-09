namespace TimeTracker.Core.DTOs.Users;

public class UpdateUserRolesDto
{
    public long UserId { get; set; }
    public List<long> RoleIds { get; set; } = new();
}