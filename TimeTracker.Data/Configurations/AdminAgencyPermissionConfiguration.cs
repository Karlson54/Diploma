using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class AdminAgencyPermissionConfiguration : IEntityTypeConfiguration<AdminAgencyPermission>
{
    public void Configure(EntityTypeBuilder<AdminAgencyPermission> builder)
    {
        builder.ToTable("admin_agency_permissions");

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

        builder.Property(e => e.AgencyId)
            .HasColumnName("agency_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.DepartmentId)
            .HasColumnName("department_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        // UpdatedAt не нужен — эту запись не обновляют, только удаляют/добавляют
        builder.Ignore(e => e.UpdatedAt);

        // Уникальность: один Admin не может иметь дубль permission на тот же отдел
        builder.HasIndex(e => new { e.UserId, e.AgencyId, e.DepartmentId })
            .HasDatabaseName("uq_admin_agency_permissions")
            .IsUnique();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_admin_agency_permissions_user_id");

        builder.HasIndex(e => e.AgencyId)
            .HasDatabaseName("ix_admin_agency_permissions_agency_id");

        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("ix_admin_agency_permissions_department_id");

        builder.HasIndex(e => e.CreatedByUserId)
            .HasDatabaseName("ix_admin_agency_permissions_created_by_user_id");

        builder.HasOne(e => e.User)
            .WithMany(u => u.AdminAgencyPermissions)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_admin_agency_permissions_user_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Agency)
            .WithMany()
            .HasForeignKey(e => e.AgencyId)
            .HasConstraintName("fk_admin_agency_permissions_agency_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .HasConstraintName("fk_admin_agency_permissions_department_id")
            .OnDelete(DeleteBehavior.Restrict);

        // CreatedByUser — без навигации обратно (чтобы не создавать лишних FK циклов)
        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .HasConstraintName("fk_admin_agency_permissions_created_by_user_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}