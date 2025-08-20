namespace TimeTracker.Data.Entities
{
    public class Agency: DictionaryEntity
    {
        public string Country { get; set; } = "Ukraine";

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}