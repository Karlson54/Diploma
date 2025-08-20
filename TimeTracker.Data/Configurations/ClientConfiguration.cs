using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

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

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(e => e.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

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
            .HasDatabaseName("uq_clients_name")
            .IsUnique();

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_clients_is_active");

        builder.HasMany(e => e.TimeEntries)
            .WithOne(t => t.Client)
            .HasForeignKey(t => t.ClientId)
            .HasConstraintName("fk_time_entries_client_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}