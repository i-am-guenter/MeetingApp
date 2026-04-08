using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;
using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

/// <summary>
/// Central DTO for moderator selection results.
/// Architectural Sync: Uses 'Id' as the primary key.
/// </summary>
public record SelectedModeratorDto(Guid Id, string Upn, string DisplayName, int NewModerationCount);

/// <summary>
/// Command to propose a moderator (stateless read operation).
/// </summary>
public record ProposeModeratorCommand() : IRequest<Result<SelectedModeratorDto>>;

public class ProposeModeratorCommandHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<ProposeModeratorCommand, Result<SelectedModeratorDto>>
{
    public async Task<Result<SelectedModeratorDto>> Handle(ProposeModeratorCommand request, CancellationToken ct)
    {
        var allColleagues = await colleagueRepository.GetActiveColleaguesAsync(ct);
        
        // Policy filters internally based on the DB-backed 'LastRejectedAt' state.
        var proposedModerator = ModeratorSelectionPolicy.SelectNext(allColleagues);

        if (proposedModerator is null)
        {
            return Result<SelectedModeratorDto>.Failure("No eligible candidates found in the pool.");
        }

        return Result<SelectedModeratorDto>.Success(new SelectedModeratorDto(
            proposedModerator.Id, 
            proposedModerator.Upn, 
            proposedModerator.DisplayName, 
            proposedModerator.ModerationCount + 1));
    }
}

/// <summary>
/// Command to finalize the selection (Atomic Commit).
/// </summary>
public record CommitModeratorCommand(Guid Id) : IRequest<Result<Unit>>;

public class CommitModeratorCommandHandler(IColleagueRepository repository) : IRequestHandler<CommitModeratorCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CommitModeratorCommand request, CancellationToken ct)
    {
        var allColleagues = await repository.GetAllColleaguesAsync(ct);
        var targetColleague = allColleagues.FirstOrDefault(c => c.Id == request.Id);

        if (targetColleague is null) return Result<Unit>.Failure("Colleague not found.");

        // Physical increment
        targetColleague.IncrementModerationCount();
        
        // Clear all session-based rejections in the pool for the next meeting.
        foreach (var colleague in allColleagues)
        {
            colleague.ClearRejection();
        }

        await repository.UpdateRangeAsync(allColleagues, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}

/// <summary>
/// Command to mark a proposed moderator as rejected in the DB.
/// </summary>
public record RejectModeratorCommand(Guid Id) : IRequest<Result<Unit>>;

public class RejectModeratorCommandHandler(IColleagueRepository repository) : IRequestHandler<RejectModeratorCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RejectModeratorCommand request, CancellationToken ct)
    {
        var allColleagues = await repository.GetActiveColleaguesAsync(ct);
        var targetColleague = allColleagues.FirstOrDefault(c => c.Id == request.Id);

        if (targetColleague is null) return Result<Unit>.Failure("Colleague not found.");

        targetColleague.MarkAsRejected();
        await repository.UpdateAsync(targetColleague, ct);
        
        return Result<Unit>.Success(Unit.Value);
    }
}

/// <summary>
/// Command to manually reset all rejections in the pool.
/// </summary>
public record ResetRejectionsCommand() : IRequest<Result<Unit>>;

public class ResetRejectionsCommandHandler(IColleagueRepository repository) : IRequestHandler<ResetRejectionsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ResetRejectionsCommand request, CancellationToken ct)
    {
        var allColleagues = await repository.GetAllColleaguesAsync(ct);
        foreach (var colleague in allColleagues)
        {
            colleague.ClearRejection();
        }

        await repository.UpdateRangeAsync(allColleagues, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}