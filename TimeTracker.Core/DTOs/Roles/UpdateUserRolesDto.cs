namespace TimeTracker.Core.DTOs.Roles;

public class UpdateUserRolesDto
{
    public List<long> RoleIds { get; set; } = new();
}