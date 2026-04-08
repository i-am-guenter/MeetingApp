using MeetingApp.Application.Moderators.Interfaces;
using MeetingApp.Domain.Moderators;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Infrastructure implementation for data access.
/// Architectural Update: Decoupled from departments. Operates on a single SQLite pool.
/// </summary>
public class ColleagueRepository(MeetingDbContext dbContext) : IColleagueRepository
{
    public async Task<List<ColleagueRecord>> GetActiveColleaguesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Colleagues
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ColleagueRecord>> GetAllColleaguesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Colleagues
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default)
    {
        await dbContext.Colleagues.AddRangeAsync(colleagues, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default)
    {
        dbContext.Colleagues.Update(colleague);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<ColleagueRecord> colleagues, CancellationToken cancellationToken = default)
    {
        dbContext.Colleagues.UpdateRange(colleagues);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // New physical delete implementation
    public async Task DeleteAsync(ColleagueRecord colleague, CancellationToken cancellationToken = default)
    {
        dbContext.Colleagues.Remove(colleague);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}