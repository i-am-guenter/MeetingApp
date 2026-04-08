using MeetingApp.Domain.Moderators;

namespace MeetingApp.Application.Moderators.Interfaces;

public interface IColleagueRepository
{
    // Removed the department filter, as the application now manages a single, consolidated pool
    Task<List<ColleagueRecord>> GetActiveColleaguesAsync(CancellationToken cancellationToken = default);
    Task<List<ColleagueRecord>> GetAllColleaguesAsync(CancellationToken cancellationToken = default);
    Task<List<ColleagueRecord>> GetActiveColleaguesByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<List<ColleagueRecord>> GetAllColleaguesByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
    Task UpdateAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default);
}