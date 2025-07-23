using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Data.Entities;

namespace TimeTracker.Data.Configurations;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("time_entries");

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

        builder.Property(e => e.EntryDate)
            .HasColumnName("entry_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.AgencyId)
            .HasColumnName("agency_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.MarketId)
            .HasColumnName("market_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.ContractingAgencyId)
            .HasColumnName("contracting_agency_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.ClientId)
            .HasColumnName("client_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.ProjectBrandId)
            .HasColumnName("project_brand_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.MediaId)
            .HasColumnName("media_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.JobTypeId)
            .HasColumnName("job_type_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.HoursMilliseconds)
            .HasColumnName("hours_milliseconds")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(e => e.Comments)
            .HasColumnName("comments")
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasCheckConstraint("ck_time_entries_hours", 
            "hours_milliseconds > 0 AND hours_milliseconds <= 86400000");
            
        builder.HasCheckConstraint("ck_time_entries_date", 
            "entry_date <= DATEADD(day, 1, GETDATE())");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_time_entries_user_id");

        builder.HasIndex(e => e.EntryDate)
            .HasDatabaseName("ix_time_entries_entry_date");

        builder.HasIndex(e => e.ClientId)
            .HasDatabaseName("ix_time_entries_client_id");

        builder.HasIndex(e => e.AgencyId)
            .HasDatabaseName("ix_time_entries_agency_id");

        builder.HasIndex(e => new { e.EntryDate, e.UserId })
            .HasDatabaseName("ix_time_entries_date_user");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("ix_time_entries_created_at");

        builder.HasOne(e => e.User)
            .WithMany(u => u.TimeEntries)
            .HasForeignKey(e => e.UserId)
            .HasConstraintName("fk_time_entries_user_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Agency)
            .WithMany(a => a.TimeEntries)
            .HasForeignKey(e => e.AgencyId)
            .HasConstraintName("fk_time_entries_agency_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Market)
            .WithMany(m => m.TimeEntries)
            .HasForeignKey(e => e.MarketId)
            .HasConstraintName("fk_time_entries_market_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractingAgency)
            .WithMany(ca => ca.TimeEntries)
            .HasForeignKey(e => e.ContractingAgencyId)
            .HasConstraintName("fk_time_entries_contracting_agency_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.TimeEntries)
            .HasForeignKey(e => e.ClientId)
            .HasConstraintName("fk_time_entries_client_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProjectBrand)
            .WithMany(pb => pb.TimeEntries)
            .HasForeignKey(e => e.ProjectBrandId)
            .HasConstraintName("fk_time_entries_project_brand_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Media)
            .WithMany(m => m.TimeEntries)
            .HasForeignKey(e => e.MediaId)
            .HasConstraintName("fk_time_entries_media_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.JobType)
            .WithMany(jt => jt.TimeEntries)
            .HasForeignKey(e => e.JobTypeId)
            .HasConstraintName("fk_time_entries_job_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}