using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class JobTypeConfiguration : IEntityTypeConfiguration<JobType>
{
    public void Configure(EntityTypeBuilder<JobType> builder)
    {
        builder.ToTable("job_types");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("uq_job_types_name")
            .IsUnique();

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_job_types_is_active");

        builder.HasMany(e => e.TimeEntries)
            .WithOne(t => t.JobType)
            .HasForeignKey(t => t.JobTypeId)
            .HasConstraintName("fk_time_entries_job_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}