namespace MeetingApp.Domain.Moderators;

public static class ModeratorSelectionPolicy
{
    /// <summary>
    /// Executes the weighted random selection based on the persistent DB state.
    /// Architectural Update: Stateless exclusion lists are replaced by DB-backed rejection tracking.
    /// </summary>
    public static ColleagueRecord? SelectNext(IEnumerable<ColleagueRecord> pool)
    {
        // We only consider active colleagues who haven't been rejected in the current selection round.
        // Rejections are persisted via the 'LastRejectedAt' property in the database.
        var eligiblePool = pool
            .Where(c => c.IsActive && c.LastRejectedAt == null)
            .ToList();
        
        if (eligiblePool.Count == 0)
        {
            return null;
        }

        // Standard logic: Find the minimum moderation count among eligible candidates.
        int minModerationCount = eligiblePool.Min(c => c.ModerationCount);
        
        var eligibleCandidates = eligiblePool
            .Where(c => c.ModerationCount == minModerationCount)
            .ToList();

        // Perform random selection among tied candidates to ensure fairness.
        var random = new Random();
        int selectedIndex = random.Next(eligibleCandidates.Count);

        return eligibleCandidates[selectedIndex];
    }
}