using MediatR;
using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.ManagePool;

public record EditColleagueCommand(string Upn, string FirstName, string LastName) : IRequest<Result<Unit>>;

public class EditColleagueCommandHandler(IColleagueRepository repository) : IRequestHandler<EditColleagueCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(EditColleagueCommand request, CancellationToken ct)
    {
        var allColleagues = await repository.GetAllColleaguesAsync(ct);
        
        // Suche den Kollegen anhand des UPN (normalized)
        var targetColleague = allColleagues.FirstOrDefault(c => c.Upn == request.Upn.ToLowerInvariant().Trim());

        if (targetColleague is null)
        {
            return Result<Unit>.Failure("Colleague not found.");
        }

        // Fix für CS1061: Die Signatur im Domain Model erwartet (upn, firstName, lastName)
        // Wir übergeben alle drei Parameter, um den Contract zu erfüllen.
        targetColleague.UpdateProfile(request.Upn, request.FirstName, request.LastName);
        
        await repository.UpdateAsync(targetColleague, ct);
        
        return Result<Unit>.Success(Unit.Value);
    }
}