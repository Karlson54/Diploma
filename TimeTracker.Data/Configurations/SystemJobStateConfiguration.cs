using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class SystemJobStateConfiguration : IEntityTypeConfiguration<SystemJobState>
{
    public void Configure(EntityTypeBuilder<SystemJobState> builder)
    {
        builder.ToTable("system_job_states");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.JobName)
            .HasColumnName("job_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastRunAt)
            .HasColumnName("last_run_at");

        builder.Property(e => e.LastStatus)
            .HasColumnName("last_status")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.LastErrorMessage)
            .HasColumnName("last_error_message")
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(e => e.JobName)
            .HasDatabaseName("uq_system_job_states_job_name")
            .IsUnique();
    }
}