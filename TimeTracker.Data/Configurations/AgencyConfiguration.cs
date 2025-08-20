using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations
{
    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.ToTable("agencies");

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

            builder.Property(e => e.Country)
                .HasColumnName("country")
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Ukraine");

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
                .HasDatabaseName("uq_agencies_name")
                .IsUnique();

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("ix_agencies_is_active");

            builder.HasMany(e => e.Users)
                .WithOne(u => u.Agency)
                .HasForeignKey(u => u.AgencyId)
                .HasConstraintName("fk_users_agency_id")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.TimeEntries)
                .WithOne(t => t.Agency)
                .HasForeignKey(t => t.AgencyId)
                .HasConstraintName("fk_time_entries_agency_id")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}