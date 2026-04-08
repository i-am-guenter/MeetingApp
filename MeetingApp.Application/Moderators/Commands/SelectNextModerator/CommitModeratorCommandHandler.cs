using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

// The Commit Command: Actually saves the incremented count for the accepted moderator
public record CommitModeratorCommand(Guid SelectedId) : IRequest<Result<Unit>>;

public class CommitModeratorCommandHandler(IColleagueRepository repository) : IRequestHandler<CommitModeratorCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CommitModeratorCommand request, CancellationToken cancellationToken)
    {
        var allColleagues = await repository.GetAllColleaguesAsync(cancellationToken);
        var targetColleague = allColleagues.FirstOrDefault(c => c.Id == request.SelectedId);

        if (targetColleague is null)
        {
            return Result<Unit>.Failure("The selected colleague could no longer be found in the database.");
        }

        // Apply the physical increment only upon explicit acceptance
        targetColleague.IncrementModerationCount();
        await repository.UpdateAsync(targetColleague, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}