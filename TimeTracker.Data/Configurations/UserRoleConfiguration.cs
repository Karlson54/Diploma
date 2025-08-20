using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(e => new { e.UserId, e.RoleId })
            .HasDatabaseName("uq_user_roles_user_role")
            .IsUnique();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_user_roles_user_id");

        builder.HasIndex(e => e.RoleId)
            .HasDatabaseName("ix_user_roles_role_id");

        builder.HasOne(e => e.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_user_roles_user_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .HasConstraintName("fk_user_roles_role_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}