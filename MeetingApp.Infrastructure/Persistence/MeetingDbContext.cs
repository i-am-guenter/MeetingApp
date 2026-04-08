using MeetingApp.Domain.Moderators;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.Infrastructure.Persistence;

public class MeetingDbContext(DbContextOptions<MeetingDbContext> options) : DbContext(options)
{
    public DbSet<ColleagueRecord> Colleagues => Set<ColleagueRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ColleagueRecord>(entity =>
        {
            // Standard Guid Primary Key
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Upn).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            
            // The UPN must be unique across the entire database to prevent duplicates
            entity.HasIndex(e => e.Upn).IsUnique();
        });
    }
}