using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

public record ToggleColleagueStatusCommand(string Upn) : IRequest<Result<Unit>>;

public class ToggleColleagueStatusCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<ToggleColleagueStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ToggleColleagueStatusCommand request, CancellationToken cancellationToken)
    {
        var allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);
        var targetUser = allColleagues.FirstOrDefault(c => c.Upn == request.Upn.ToLowerInvariant().Trim());

        if (targetUser is null)
        {
            return Result<Unit>.Failure("Colleague not found in the database.");
        }

        if (targetUser.IsActive)
        {
            targetUser.Deactivate();
        }
        else
        {
            targetUser.Reactivate();
        }

        await colleagueRepository.UpdateAsync(targetUser, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}