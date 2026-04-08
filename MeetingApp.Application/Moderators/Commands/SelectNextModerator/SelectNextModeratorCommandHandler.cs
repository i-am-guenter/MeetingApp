using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

public class SelectNextModeratorCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<SelectNextModeratorCommand, Result<SelectedModeratorDto>>
{
    public async Task<Result<SelectedModeratorDto>> Handle(SelectNextModeratorCommand request, CancellationToken cancellationToken)
    {
        // Fetching all colleagues to ensure the policy can evaluate correctly
        List<ColleagueRecord> allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);

        // Architectural Fix: Convert the incoming List to a HashSet for O(1) lookups and pass it to the policy
        var excludedSet = new HashSet<Guid>(request.ExcludedIds);
        
        ColleagueRecord? selectedModerator = ModeratorSelectionPolicy.SelectNext(allColleagues, excludedSet);

        if (selectedModerator is null)
        {
            return Result<SelectedModeratorDto>.Failure("No eligible active colleagues found (or all active candidates were explicitly rejected).");
        }

        selectedModerator.IncrementModerationCount();
        await colleagueRepository.UpdateAsync(selectedModerator, cancellationToken);

        var dto = new SelectedModeratorDto(
            selectedModerator.Id, 
            selectedModerator.Upn, 
            selectedModerator.DisplayName, 
            selectedModerator.ModerationCount);

        return Result<SelectedModeratorDto>.Success(dto);
    }
}