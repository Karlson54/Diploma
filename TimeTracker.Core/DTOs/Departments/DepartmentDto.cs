namespace TimeTracker.Core.DTOs.Departments;

public class DepartmentDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}