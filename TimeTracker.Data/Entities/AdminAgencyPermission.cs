namespace TimeTracker.Data.Entities;

public class AdminAgencyPermission : BaseEntity
{
    public long UserId { get; set; }
    public long AgencyId { get; set; }
    public long DepartmentId { get; set; }
    public long CreatedByUserId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Agency Agency { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
}