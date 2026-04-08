using MediatR;
using MeetingApp.Application.Moderators.Interfaces;

namespace MeetingApp.Application.Moderators.Queries.GetFullPool;

// Architectural Update: The DTO now carries the IsActive state and the Guid Id.
public record PoolMemberDto(Guid Id, string DisplayName, string Upn, string FirstName, string LastName, int ModerationCount, bool IsActive);

public record GetFullPoolQuery() : IRequest<List<PoolMemberDto>>;

public class GetFullPoolQueryHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<GetFullPoolQuery, List<PoolMemberDto>>
{
    public async Task<List<PoolMemberDto>> Handle(GetFullPoolQuery request, CancellationToken cancellationToken)
    {
        // We purposefully fetch ALL colleagues here, not just active ones.
        var allColleagues = await colleagueRepository.GetAllColleaguesAsync(cancellationToken);

        return allColleagues
            .OrderByDescending(c => c.IsActive) // Active users first
            .ThenBy(c => c.DisplayName)
            .Select(c => new PoolMemberDto(c.Id, c.DisplayName, c.Upn, c.FirstName, c.LastName, c.ModerationCount, c.IsActive))
            .ToList();
    }
}