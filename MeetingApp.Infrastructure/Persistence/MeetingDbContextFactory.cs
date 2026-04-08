using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeetingApp.Infrastructure.Persistence;

/// <summary>
/// This factory is ONLY used by the Entity Framework Core CLI tools at design time
/// (e.g., when running 'dotnet ef migrations add'). It provides an explicit way 
/// to create the DbContext, bypassing the Web project's dependency injection container.
/// </summary>
public class MeetingDbContextFactory : IDesignTimeDbContextFactory<MeetingDbContext>
{
    public MeetingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MeetingDbContext>();
        
        // We configure the SQLite connection explicitly for the CLI tooling.
        // The actual application runtime will still use the configuration from Program.cs.
        optionsBuilder.UseSqlite("Data Source=meetingapp.db", sqlOptions => 
        {
            sqlOptions.MigrationsAssembly("MeetingApp.Infrastructure");
        });

        return new MeetingDbContext(optionsBuilder.Options);
    }
}