namespace TimeTracker.Core.DTOs.AdminPermissions;

public class AdminPermissionsForUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<AdminPermissionDto> Permissions { get; set; } = new();
}