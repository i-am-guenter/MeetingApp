using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

// The Proposal Command: Calculates the next moderator, but DOES NOT save anything yet.
public record ProposeModeratorCommand(List<Guid> ExcludedIds) : IRequest<Result<SelectedModeratorDto>>;

public class ProposeModeratorCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<ProposeModeratorCommand, Result<SelectedModeratorDto>>
{
    public async Task<Result<SelectedModeratorDto>> Handle(ProposeModeratorCommand request, CancellationToken cancellationToken)
    {
        List<ColleagueRecord> allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);
        var excludedSet = new HashSet<Guid>(request.ExcludedIds);
        
        ColleagueRecord? proposedModerator = ModeratorSelectionPolicy.SelectNext(allColleagues, excludedSet);

        if (proposedModerator is null)
        {
            return Result<SelectedModeratorDto>.Failure("No eligible active colleagues found (or all active candidates were explicitly rejected).");
        }

        // Notice: No IncrementModerationCount() and no UpdateAsync() here! 
        // This is a pure read operation for the proposal.
        
        var dto = new SelectedModeratorDto(
            proposedModerator.Id, 
            proposedModerator.Upn, 
            proposedModerator.DisplayName, 
            proposedModerator.ModerationCount + 1); // We show what the count WILL be

        return Result<SelectedModeratorDto>.Success(dto);
    }
}