namespace TimeTracker.Data.Entities;

public class Role : DictionaryEntity
{
    public string? Description { get; set; }
    public string? Permissions { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}