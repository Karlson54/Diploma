namespace TimeTracker.Data.Entities;

public class Media : DictionaryEntity
{
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}