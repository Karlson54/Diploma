namespace TimeTracker.Core.DTOs.AdminPermissions;

public class AdminPermissionDto
{
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public long DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}