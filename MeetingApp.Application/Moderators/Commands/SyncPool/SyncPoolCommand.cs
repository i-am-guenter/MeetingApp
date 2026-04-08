using MediatR;
using MeetingApp.Domain.Common;

namespace MeetingApp.Application.Moderators.Commands.SyncPool;

public record SyncPoolResultDto(int AddedCount, int DeactivatedCount, int ReactivatedCount);

// No parameters needed anymore, since the configuration is injected directly into the handler
public record SyncPoolCommand() : IRequest<Result<SyncPoolResultDto>>;