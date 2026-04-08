using MediatR;
using MeetingApp.Application.Moderators.Interfaces;

namespace MeetingApp.Application.Moderators.Queries.GetActivePool;

public class GetActivePoolQueryHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<GetActivePoolQuery, List<PoolMemberDto>>
{
    public async Task<List<PoolMemberDto>> Handle(GetActivePoolQuery request, CancellationToken cancellationToken)
    {
        var activeColleagues = await colleagueRepository.GetActiveColleaguesAsync(cancellationToken);

        // Map domain entities to DTOs for presentation, ordered alphabetically for the UI grid
        return activeColleagues
            .OrderBy(c => c.DisplayName)
            .Select(c => new PoolMemberDto(c.DisplayName, c.Email, c.ModerationCount))
            .ToList();
    }
}