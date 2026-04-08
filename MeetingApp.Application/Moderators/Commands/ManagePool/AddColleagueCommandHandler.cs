using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

/// <summary>
/// Command to add a new colleague to the pool.
/// </summary>
public record AddColleagueCommand(string Upn, string FirstName, string LastName) : IRequest<Result<Unit>>;

public class AddColleagueCommandHandler(IColleagueRepository repository) 
    : IRequestHandler<AddColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddColleagueCommand request, CancellationToken ct)
    {
        var normalizedUpn = request.Upn.ToLowerInvariant().Trim();
        var allColleagues = await repository.GetAllColleaguesAsync(ct);
        var existingRecord = allColleagues.FirstOrDefault(c => c.Upn == normalizedUpn);

        if (existingRecord is not null)
        {
            if (existingRecord.IsActive) 
            {
                return Result<Unit>.Failure("User is already an active member of the pool.");
            }
            
            // Reactivate soft-deleted user
            existingRecord.Reactivate();
            
            // Synchronized update call with 3 arguments to match the unified Domain behavior
            existingRecord.UpdateProfile(request.Upn, request.FirstName, request.LastName);
            
            await repository.UpdateAsync(existingRecord, ct);
            return Result<Unit>.Success(Unit.Value);
        }

        // Architectural Fairness: New members start with the current maximum count
        int currentMaxCount = allColleagues.Count != 0 && allColleagues.Any(c => c.IsActive)
            ? allColleagues.Where(c => c.IsActive).Max(c => c.ModerationCount) 
            : 0;

        var newRecord = new ColleagueRecord(
            upn: normalizedUpn, 
            firstName: request.FirstName, 
            lastName: request.LastName,
            initialModerationCount: currentMaxCount);

        await repository.AddRangeAsync(new[] { newRecord }, ct);
        
        return Result<Unit>.Success(Unit.Value);
    }
}