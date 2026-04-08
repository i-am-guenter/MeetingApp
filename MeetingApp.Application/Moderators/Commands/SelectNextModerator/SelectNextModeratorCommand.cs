using MediatR;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

// Architectural Update: Removed EntraObjectId, utilizing the local Guid Id.
public record SelectedModeratorDto(Guid Id, string Upn, string DisplayName, int NewModerationCount);

// Architectural Update: Removed 'Department' and injected the 'ExcludedIds' state tracker 
// required for the stateless Reject/Re-Roll feature from the UI.
public record SelectNextModeratorCommand(List<Guid> ExcludedIds) : IRequest<Result<SelectedModeratorDto>>;