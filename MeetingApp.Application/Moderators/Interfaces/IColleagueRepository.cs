using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Interfaces;

public interface IColleagueRepository
{
    // Architectural Update: Removed department parameters to reflect the single-pool architecture.
    Task<List<ColleagueRecord>> GetActiveColleaguesAsync(CancellationToken cancellationToken = default);
    Task<List<ColleagueRecord>> GetAllColleaguesAsync(CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
    Task UpdateAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
}