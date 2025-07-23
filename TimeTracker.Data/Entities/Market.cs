namespace TimeTracker.Data.Entities;

public class Market: DictionaryEntity
{
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}