using MeetingApp.Application.Moderators.Dtos;

namespace MeetingApp.Application.Moderators.Interfaces;

public interface IGraphService
{
    Task<GraphUserDto?> GetUserAsync(Guid entraObjectId, CancellationToken cancellationToken = default);
    Task<List<GraphUserDto>> GetUsersByUpnsAsync(string[] upns, CancellationToken cancellationToken = default);
}

/*using MeetingApp.Application.Moderators.Dtos;

namespace MeetingApp.Application.Moderators.Interfaces;

public interface IGraphService
{
    Task<GraphUserDto?> GetUserAsync(Guid entraObjectId, CancellationToken cancellationToken = default);
    Task<List<GraphUserDto>> GetUsersByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<List<GraphUserDto>> GetUsersByDepartmentsAsync(string[] departments, CancellationToken cancellationToken = default);
    Task<List<GraphUserDto>> GetUsersByUpnsAsync(string[] upns, CancellationToken cancellationToken = default);
}
*/