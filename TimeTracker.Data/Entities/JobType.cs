namespace TimeTracker.Data.Entities;

public class JobType : DictionaryEntity
{
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}