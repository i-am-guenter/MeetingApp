using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

public class SelectNextModeratorCommandHandler(
    IColleagueRepository colleagueRepository,
    IGraphService graphService) 
    : IRequestHandler<SelectNextModeratorCommand, Result<SelectedModeratorDto>>
{
    public async Task<Result<SelectedModeratorDto>> Handle(SelectNextModeratorCommand request, CancellationToken cancellationToken)
    {
        // Architectural update: Fetching the entire consolidated pool instead of a specific department
        List<ColleagueRecord> activeColleagues = await colleagueRepository.GetActiveColleaguesAsync(cancellationToken);

        if (activeColleagues.Count == 0)
        {
            return Result<SelectedModeratorDto>.Failure("No active colleagues found in the meeting pool. Please trigger a synchronization.");
        }

        ColleagueRecord? selectedModerator = ModeratorSelectionPolicy.SelectNext(activeColleagues);

        if (selectedModerator is null)
        {
            return Result<SelectedModeratorDto>.Failure("An unexpected error occurred during the mathematical selection policy evaluation.");
        }

        selectedModerator.IncrementModerationCount();
        await colleagueRepository.UpdateAsync(selectedModerator, cancellationToken);

        var graphUser = await graphService.GetUserAsync(selectedModerator.EntraObjectId, cancellationToken);
        
        if (graphUser is null)
        {
             return Result<SelectedModeratorDto>.Failure($"Moderator {selectedModerator.EntraObjectId} was selected but could not be resolved in Entra ID.");
        }

        var dto = new SelectedModeratorDto(
            graphUser.EntraObjectId, 
            graphUser.Upn, 
            graphUser.DisplayName, 
            selectedModerator.ModerationCount);

        return Result<SelectedModeratorDto>.Success(dto);
    }
}