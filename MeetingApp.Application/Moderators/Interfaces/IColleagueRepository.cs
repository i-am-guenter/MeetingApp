using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Interfaces;

public interface IColleagueRepository
{
    // Fetches only active colleagues for the selection pool
    Task<List<ColleagueRecord>> GetActiveColleaguesAsync(CancellationToken cancellationToken = default);
    
    // Fetches all colleagues (including inactive ones) for the UI management grid
    Task<List<ColleagueRecord>> GetAllColleaguesAsync(CancellationToken cancellationToken = default);
    
    Task AddRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
    Task UpdateAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
    
    // Architectural Fix: Added the physical delete contract for the Hard Delete feature
    Task DeleteAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default);
}