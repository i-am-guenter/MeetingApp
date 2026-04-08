using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

// Architectural Update: The Handler relies 100% on the local SQLite DB.
// IGraphService injection is removed.
public class SelectNextModeratorCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<SelectNextModeratorCommand, Result<SelectedModeratorDto>>
{
    public async Task<Result<SelectedModeratorDto>> Handle(SelectNextModeratorCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the Source of Truth from the local database
        List<ColleagueRecord> activeColleagues = await colleagueRepository.GetActiveColleaguesAsync(cancellationToken);

        if (activeColleagues.Count == 0)
        {
            return Result<SelectedModeratorDto>.Failure("No active colleagues found in the meeting pool. Please add members via the Pool Management UI.");
        }

        // 2. Execute the domain policy (mathematical selection)
        ColleagueRecord? selectedModerator = ModeratorSelectionPolicy.SelectNext(activeColleagues);

        if (selectedModerator is null)
        {
            return Result<SelectedModeratorDto>.Failure("An unexpected error occurred during the mathematical selection policy evaluation.");
        }

        // 3. Mutate state and persist
        selectedModerator.IncrementModerationCount();
        await colleagueRepository.UpdateAsync(selectedModerator, cancellationToken);

        // 4. Map to DTO and return
        // We use the local Id and Upn from the refactored ColleagueRecord
        var dto = new SelectedModeratorDto(
            selectedModerator.Id, 
            selectedModerator.Upn, 
            selectedModerator.DisplayName, 
            selectedModerator.ModerationCount);

        return Result<SelectedModeratorDto>.Success(dto);
    }
}