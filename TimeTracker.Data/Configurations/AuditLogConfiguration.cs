using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.UserName)
            .HasColumnName("user_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Action)
            .HasColumnName("action")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EntityName)
            .HasColumnName("entity_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id")
            .HasColumnType("bigint");

        builder.Property(e => e.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .IsRequired()
            .HasMaxLength(45); // IPv6

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Success)
            .HasColumnName("success")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        // Індекси для швидкого пошуку
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_audit_logs_user_id");

        builder.HasIndex(e => e.Action)
            .HasDatabaseName("ix_audit_logs_action");

        builder.HasIndex(e => e.EntityName)
            .HasDatabaseName("ix_audit_logs_entity_name");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_audit_logs_created_at");

        builder.HasIndex(e => new { e.EntityName, e.EntityId })
            .HasDatabaseName("ix_audit_logs_entity");

        builder.HasIndex(e => new { e.UserId, e.CreatedAt })
            .HasDatabaseName("ix_audit_logs_user_date");
    }
}