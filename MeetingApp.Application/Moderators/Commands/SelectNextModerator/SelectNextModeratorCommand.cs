using MediatR;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

/// <summary>
/// The DTO representing the result of the selection process.
/// Placed here to ensure high cohesion with the Command it belongs to.
/// </summary>
public record SelectedModeratorDto(Guid EntraObjectId, string Upn, string DisplayName, int NewModerationCount);

/// <summary>
/// The command to trigger the selection of the next moderator based on the globally configured pool.
/// </summary>
public record SelectNextModeratorCommand() : IRequest<Result<SelectedModeratorDto>>;