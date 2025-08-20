using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class ContractingAgencyConfiguration : IEntityTypeConfiguration<ContractingAgency>
{
    public void Configure(EntityTypeBuilder<ContractingAgency> builder)
    {
        builder.ToTable("contracting_agencies");

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
            .HasDatabaseName("uq_contracting_agencies_name")
            .IsUnique();

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_contracting_agencies_is_active");

        builder.HasMany(e => e.TimeEntries)
            .WithOne(t => t.ContractingAgency)
            .HasForeignKey(t => t.ContractingAgencyId)
            .HasConstraintName("fk_time_entries_contracting_agency_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}