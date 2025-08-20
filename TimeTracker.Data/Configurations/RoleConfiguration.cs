using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.Property(e => e.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("nvarchar(max)");

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
            .HasDatabaseName("uq_roles_name")
            .IsUnique();

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_roles_is_active");

        builder.HasCheckConstraint("ck_roles_permissions", 
            "permissions IS NULL OR ISJSON(permissions) = 1");

        builder.HasMany(e => e.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .HasConstraintName("fk_user_roles_role_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}