using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

/// <summary>
/// Command to toggle the active state of a colleague in the pool.
/// (Point 4 of the requested feature set - Soft Delete / Graying out)
/// </summary>
public record ToggleColleagueStatusCommand(string Upn) : IRequest<Result<Unit>>;

public class ToggleColleagueStatusCommandHandler(IColleagueRepository repository) : IRequestHandler<ToggleColleagueStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ToggleColleagueStatusCommand request, CancellationToken cancellationToken)
    {
        // Fetch all colleagues to find the correct entity
        var allColleagues = await repository.GetAllColleaguesAsync(cancellationToken);
        
        // Find the specific colleague by their unique business identifier
        var targetColleague = allColleagues.FirstOrDefault(c => c.Upn == request.Upn.ToLowerInvariant().Trim());

        if (targetColleague is null)
        {
            return Result<Unit>.Failure($"Colleague with UPN '{request.Upn}' could not be found in the database.");
        }

        // Toggle the internal state via Domain Behavior
        if (targetColleague.IsActive)
        {
            targetColleague.Deactivate();
        }
        else
        {
            targetColleague.Reactivate();
        }

        // Persist the state change
        await repository.UpdateAsync(targetColleague, cancellationToken);
        
        return Result<Unit>.Success(Unit.Value);
    }
}