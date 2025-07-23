namespace TimeTracker.Data.Entities;

public class ProjectBrand : DictionaryEntity
{
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}