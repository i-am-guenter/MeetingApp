using MediatR;

namespace MeetingApp.Application.Moderators.Queries.GetActivePool;

public record PoolMemberDto(string DisplayName, string Upn, int ModerationCount);

public record GetActivePoolQuery() : IRequest<List<PoolMemberDto>>;