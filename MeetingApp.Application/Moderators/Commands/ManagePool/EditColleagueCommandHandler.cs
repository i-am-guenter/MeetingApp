using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

// Consolidated file for new pool management features to maintain a clean directory structure.

// 1. Edit Feature
public record EditColleagueCommand(string OriginalUpn, string NewUpn, string FirstName, string LastName) : IRequest<Result<Unit>>;
public class EditColleagueCommandHandler(IColleagueRepository repository) : IRequestHandler<EditColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(EditColleagueCommand request, CancellationToken ct)
    {
        var all = await repository.GetAllColleaguesAsync(ct);
        var target = all.FirstOrDefault(c => c.Upn == request.OriginalUpn.ToLowerInvariant());
        if (target is null) return Result<Unit>.Failure("Colleague not found.");

        // Check if the new UPN belongs to someone else
        if (request.OriginalUpn != request.NewUpn && all.Any(c => c.Upn == request.NewUpn.ToLowerInvariant()))
        {
            return Result<Unit>.Failure("The new UPN is already assigned to another colleague.");
        }

        target.UpdateProfile(request.NewUpn, request.FirstName, request.LastName);
        await repository.UpdateAsync(target, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}

// 2. Hard Delete Feature
public record DeleteColleagueCommand(string Upn) : IRequest<Result<Unit>>;
public class DeleteColleagueCommandHandler(IColleagueRepository repository) : IRequestHandler<DeleteColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteColleagueCommand request, CancellationToken ct)
    {
        var all = await repository.GetAllColleaguesAsync(ct);
        var target = all.FirstOrDefault(c => c.Upn == request.Upn.ToLowerInvariant());
        if (target is null) return Result<Unit>.Failure("Colleague not found.");

        await repository.DeleteAsync(target, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}

// 3. Reset All Feature
public record ResetAllModeratorCountsCommand() : IRequest<Result<Unit>>;
public class ResetAllModeratorCountsCommandHandler(IColleagueRepository repository) : IRequestHandler<ResetAllModeratorCountsCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ResetAllModeratorCountsCommand request, CancellationToken ct)
    {
        var all = await repository.GetAllColleaguesAsync(ct);
        foreach (var c in all) c.ResetModerationCount();
        
        await repository.UpdateRangeAsync(all, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}

// 4. Undo Feature (for the Reject flow)
public record UndoModerationCommand(Guid Id) : IRequest<Result<Unit>>;
public class UndoModerationCommandHandler(IColleagueRepository repository) : IRequestHandler<UndoModerationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UndoModerationCommand request, CancellationToken ct)
    {
        var all = await repository.GetAllColleaguesAsync(ct);
        var target = all.FirstOrDefault(c => c.Id == request.Id);
        if (target is null) return Result<Unit>.Failure("Colleague not found.");

        target.UndoModerationCount();
        await repository.UpdateAsync(target, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}