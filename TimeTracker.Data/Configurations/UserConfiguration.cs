using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

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
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Login) 
            .HasColumnName("login")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.AgencyId)
            .HasColumnName("agency_id")
            .HasColumnType("bigint")
            .IsRequired();

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

        builder.HasIndex(e => e.Email)
            .HasDatabaseName("uq_users_email")
            .IsUnique();

        builder.HasIndex(e => e.Login)
            .HasDatabaseName("uq_users_login")
            .IsUnique();

        builder.HasIndex(e => e.AgencyId)
            .HasDatabaseName("ix_users_agency_id");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_users_is_active");

        builder.HasIndex(e => e.Login)
            .HasDatabaseName("ix_users_login");

        builder.HasOne(e => e.Agency)
            .WithMany(a => a.Users)
            .HasForeignKey(e => e.AgencyId)
            .HasConstraintName("fk_users_agency_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .HasConstraintName("fk_user_roles_user_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.TimeEntries)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("fk_time_entries_user_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}