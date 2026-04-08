using MediatR;
using MeetingApp.Application.Configuration;
using MeetingApp.Application.Moderators.Commands.SyncPool;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeetingApp.Web.HostedServices;

/// <summary>
/// Executes the database synchronization against Entra ID during application startup.
/// Implements IHostedService to run purely in the background before HTTP traffic is accepted by Kestrel.
/// </summary>
public class StartupSyncService(
    IServiceProvider serviceProvider, 
    IOptions<MeetingSettings> settings, 
    ILogger<StartupSyncService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string[] targetDepartments = settings.Value.IncludedDepartments;

        if (targetDepartments == null || targetDepartments.Length == 0)
        {
            logger.LogWarning("No target departments configured in MeetingSettings. Startup synchronization skipped.");
            return;
        }

        // Architectural Requirement:
        // IHostedService is registered as a Singleton. MediatR and the DbContext are registered as Scoped services.
        // We must create an explicit IServiceScope to safely resolve and execute our scoped commands
        // without encountering an InvalidOperationException from the DI container.
        using IServiceScope scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        foreach (string department in targetDepartments)
        {
            logger.LogInformation("Starting cold-start synchronization for department: {Department}", department);
            
            // Dispatching the command based on your current local implementation (SyncDepartmentCommand)
            var command = new SyncPoolCommand();
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                logger.LogInformation(
                    "Sync completed for {Department}. Added: {Added}, Deactivated: {Deactivated}, Reactivated: {Reactivated}", 
                    department, result.Value.AddedCount, result.Value.DeactivatedCount, result.Value.ReactivatedCount);
            }
            else
            {
                logger.LogError("Sync failed for department {Department}: {Error}", department, result.ErrorMessage);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}