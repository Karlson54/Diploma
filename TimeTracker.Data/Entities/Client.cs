namespace TimeTracker.Data.Entities;

public class Client : DictionaryEntity
{
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}