using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

// The UI now dictates the full dataset
public record AddColleagueCommand(string Upn, string FirstName, string LastName) : IRequest<Result<Unit>>;

public class AddColleagueCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<AddColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddColleagueCommand request, CancellationToken cancellationToken)
    {
        string normalizedUpn = request.Upn.ToLowerInvariant().Trim();
        
        // 1. Check for duplicates or soft-deleted users
        var allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);
        var existingRecord = allColleagues.FirstOrDefault(c => c.Upn == normalizedUpn);

        if (existingRecord is not null)
        {
            if (existingRecord.IsActive)
            {
                return Result<Unit>.Failure($"A colleague with UPN '{normalizedUpn}' is already active in the pool.");
            }
            
            // Reactivate soft-deleted user and update name if it changed during their absence
            existingRecord.Reactivate();
            existingRecord.UpdateProfile(request.FirstName, request.LastName);
            await colleagueRepository.UpdateAsync(existingRecord, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }

        // 2. Add brand new user
        // Architectural fairness constraint: New joiners inherit the current highest moderation count
        // so they don't get spammed by the algorithm immediately.
        int currentMaxModerationCount = allColleagues.Count != 0 && allColleagues.Any(c => c.IsActive)
            ? allColleagues.Where(c => c.IsActive).Max(c => c.ModerationCount) 
            : 0;

        var newColleague = new ColleagueRecord(
            upn: normalizedUpn,
            firstName: request.FirstName,
            lastName: request.LastName,
            initialModerationCount: currentMaxModerationCount);

        // We use AddRangeAsync as defined in your provided IColleagueRepository
        await colleagueRepository.AddRangeAsync([newColleague], cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}