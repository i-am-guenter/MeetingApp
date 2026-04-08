using MediatR;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.SelectNextModerator;

// The Data Transfer Object (DTO) returning the result to the UI.
// Completely detached from Entra ID, using the local Guid Id.
public record SelectedModeratorDto(Guid Id, string Upn, string DisplayName, int NewModerationCount);

// The Command triggering the selection process. No parameters required.
public record SelectNextModeratorCommand() : IRequest<Result<SelectedModeratorDto>>;