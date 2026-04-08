using MediatR;
using MeetingApp.Application.Moderators.Interfaces;

namespace MeetingApp.Application.Moderators.Queries.GetActivePool;

public class GetActivePoolQueryHandler(IColleagueRepository colleagueRepository) 
    : IRequestHandler<GetActivePoolQuery, List<PoolMemberDto>>
{
    public async Task<List<PoolMemberDto>> Handle(GetActivePoolQuery request, CancellationToken cancellationToken)
    {
        var activeColleagues = await colleagueRepository.GetActiveColleaguesAsync(cancellationToken);

        // Architectural Fix: Changed 'c.Email' to 'c.Upn' to match the updated Domain Model
        return activeColleagues
            .OrderBy(c => c.DisplayName)
            .Select(c => new PoolMemberDto(c.DisplayName, c.Upn, c.ModerationCount))
            .ToList();
    }
}