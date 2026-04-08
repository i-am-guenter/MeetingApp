using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

// Assuming the Command record is defined in the same file or a corresponding one.
public record AddColleagueCommand(string Upn, string FirstName, string LastName) : IRequest<Result<Unit>>;

public class AddColleagueCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<AddColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddColleagueCommand request, CancellationToken cancellationToken)
    {
        string normalizedUpn = request.Upn.ToLowerInvariant().Trim();
        
        var allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);
        var existingRecord = allColleagues.FirstOrDefault(c => c.Upn == normalizedUpn);

        if (existingRecord is not null)
        {
            if (existingRecord.IsActive)
            {
                return Result<Unit>.Failure($"A colleague with UPN '{normalizedUpn}' is already active in the pool.");
            }
            
            existingRecord.Reactivate();
            
            // Architectural Fix: Corrected method signature to match the updated Domain Model (3 arguments)
            existingRecord.UpdateProfile(request.Upn, request.FirstName, request.LastName);
            
            await colleagueRepository.UpdateAsync(existingRecord, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }

        int currentMaxModerationCount = allColleagues.Count != 0 && allColleagues.Any(c => c.IsActive)
            ? allColleagues.Where(c => c.IsActive).Max(c => c.ModerationCount) 
            : 0;

        var newColleague = new ColleagueRecord(
            upn: normalizedUpn,
            firstName: request.FirstName,
            lastName: request.LastName,
            initialModerationCount: currentMaxModerationCount);

        await colleagueRepository.AddRangeAsync([newColleague], cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}