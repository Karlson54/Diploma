using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

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

        builder.Property(e => e.AgencyId)
            .HasColumnName("agency_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        // Унікальність назви в межах агенції
        builder.HasIndex(e => new { e.AgencyId, e.Name })
            .HasDatabaseName("uq_departments_agency_name")
            .IsUnique();

        builder.HasIndex(e => e.AgencyId)
            .HasDatabaseName("ix_departments_agency_id");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_departments_is_active");

        builder.HasOne(e => e.Agency)
            .WithMany(a => a.Departments)
            .HasForeignKey(e => e.AgencyId)
            .HasConstraintName("fk_departments_agency_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Users)
            .WithOne(u => u.Department)
            .HasForeignKey(u => u.DepartmentId)
            .HasConstraintName("fk_users_department_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.TimeEntries)
            .WithOne(te => te.Department)
            .HasForeignKey(te => te.DepartmentId)
            .HasConstraintName("fk_time_entries_department_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}