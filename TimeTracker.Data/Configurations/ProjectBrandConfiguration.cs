using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class ProjectBrandConfiguration : IEntityTypeConfiguration<ProjectBrand>
{
    public void Configure(EntityTypeBuilder<ProjectBrand> builder)
    {
        builder.ToTable("project_brands");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

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
            .HasDatabaseName("uq_project_brands_name")
            .IsUnique();

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_project_brands_is_active");

        builder.HasMany(e => e.TimeEntries)
            .WithOne(t => t.ProjectBrand)
            .HasForeignKey(t => t.ProjectBrandId)
            .HasConstraintName("fk_time_entries_project_brand_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}