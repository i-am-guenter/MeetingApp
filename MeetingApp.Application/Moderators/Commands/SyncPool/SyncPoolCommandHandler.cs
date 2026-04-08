using MediatR;
using MeetingApp.Application.Configuration;
using MeetingApp.Application.Moderators.Dtos;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeetingApp.Application.Moderators.Commands.SyncPool;

public class SyncPoolCommandHandler(
    IColleagueRepository colleagueRepository,
    IGraphService graphService,
    IOptions<MeetingSettings> settings,
    ILogger<SyncPoolCommandHandler> logger) 
    : IRequestHandler<SyncPoolCommand, Result<SyncPoolResultDto>>
{
    public async Task<Result<SyncPoolResultDto>> Handle(SyncPoolCommand request, CancellationToken cancellationToken)
    {
        MeetingSettings currentSettings = settings.Value;
        string[] targetUpns = currentSettings.IncludedUpns;

        logger.LogInformation("SyncPoolCommand triggered. Configured UPNs in appsettings: {Count}", targetUpns.Length);

        if (targetUpns.Length == 0)
        {
            logger.LogWarning("The IncludedUpns array is empty. Please check your appsettings.json syntax and binding.");
            return Result<SyncPoolResultDto>.Success(new SyncPoolResultDto(0, 0, 0));
        }

        // 1. Fetch specific users directly via UPN
        List<GraphUserDto> specificUsers = await graphService.GetUsersByUpnsAsync(targetUpns, cancellationToken);

        logger.LogInformation("Graph API returned {Count} valid users out of {Requested} requested identifiers.", specificUsers.Count, targetUpns.Length);

        // Telemetry: Identify which exact UPNs failed to resolve in Entra ID
        if (specificUsers.Count < targetUpns.Length)
        {
            var foundUpns = specificUsers.Select(u => u.Upn.ToLowerInvariant()).ToHashSet();
            var missingUpns = targetUpns.Where(u => !foundUpns.Contains(u.ToLowerInvariant()));
            logger.LogWarning("The following Identifiers were NOT found in Entra ID (404 Not Found): {MissingUpns}", string.Join(", ", missingUpns));
        }

        // 2. Architectural core: Deduplicate in memory
        List<GraphUserDto> mergedEntraPool = specificUsers
            .DistinctBy(u => u.EntraObjectId)
            .ToList();

        // 3. Fetch current state from local DB
        // Architectural Note: We fetch ALL colleagues, as we operate on a single-pool setup now.
        List<ColleagueRecord> localColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);

        int currentMaxModerationCount = localColleagues.Count != 0 
            ? localColleagues.Max(c => c.ModerationCount) 
            : 0;

        List<ColleagueRecord> recordsToAdd = [];
        List<ColleagueRecord> recordsToUpdate = [];
        int addedCount = 0;
        int deactivatedCount = 0;
        int reactivatedCount = 0;

        // 4. Process additions and reactivations
        foreach (GraphUserDto entraUser in mergedEntraPool)
        {
            ColleagueRecord? localMatch = localColleagues.FirstOrDefault(c => c.EntraObjectId == entraUser.EntraObjectId);

            if (localMatch is null)
            {
                // Architectural Fix: Reverted to the original ColleagueRecord constructor 
                // to avoid forcing an Entity Framework Core DB Migration.
                // We strictly use C# Named Arguments to prevent CS8323 syntax errors.
                var newColleague = new ColleagueRecord(
                    entraObjectId: entraUser.EntraObjectId, 
                    firstName: entraUser.FirstName,
                    lastName: entraUser.LastName,
                    displayName: entraUser.DisplayName,
                    email: entraUser.Email,
                    department: "FixedPool", // Legacy field maintained for EF Core compatibility
                    isManuallyAdded: false, 
                    initialModerationCount: currentMaxModerationCount,
                    profilePictureUri: entraUser.ProfilePictureUri);
                    
                recordsToAdd.Add(newColleague);
                addedCount++;
            }
            else 
            {
                // Keeping the existing legacy department value intact during updates
                localMatch.UpdateProfile(
                    firstName: entraUser.FirstName, 
                    lastName: entraUser.LastName, 
                    displayName: entraUser.DisplayName, 
                    email: entraUser.Email, 
                    department: localMatch.Department, 
                    profilePictureUri: entraUser.ProfilePictureUri);

                if (!localMatch.IsActive)
                {
                    localMatch.Reactivate();
                    reactivatedCount++;
                }
                
                recordsToUpdate.Add(localMatch);
            }
        }

        // 5. Process leavers or excluded users (soft delete)
        var entraUserIds = mergedEntraPool.Select(u => u.EntraObjectId).ToHashSet();
        
        foreach (ColleagueRecord local in localColleagues)
        {
            if (!entraUserIds.Contains(local.EntraObjectId) && local.IsActive)
            {
                local.Deactivate();
                recordsToUpdate.Add(local);
                deactivatedCount++;
            }
        }

        // 6. Persist changes
        if (recordsToAdd.Count != 0)
        {
            await colleagueRepository.AddRangeAsync(recordsToAdd, cancellationToken);
            logger.LogInformation("Successfully inserted {Count} new colleagues into the database.", recordsToAdd.Count);
        }

        if (recordsToUpdate.Count != 0)
        {
            await colleagueRepository.UpdateRangeAsync(recordsToUpdate, cancellationToken);
            logger.LogInformation("Successfully updated {Count} existing colleagues in the database.", recordsToUpdate.Count);
        }

        return Result<SyncPoolResultDto>.Success(new SyncPoolResultDto(addedCount, deactivatedCount, reactivatedCount));
    }
}