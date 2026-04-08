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
            entity.HasKey(e => e.EntraObjectId);
            entity.Property(e => e.EntraObjectId).ValueGeneratedNever();
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProfilePictureUri).HasMaxLength(1000);
            
            entity.HasIndex(e => new { e.Department, e.IsActive });
            entity.HasIndex(e => e.Email);
        });
    }
}