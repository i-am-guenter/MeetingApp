namespace MeetingApp.Domain.Moderators;

public static class ModeratorSelectionPolicy
{
    // Architectural Update: The policy now respects an exclusion list for the "Reject" feature
    public static ColleagueRecord? SelectNext(IEnumerable<ColleagueRecord> pool, HashSet<Guid> excludedIds)
    {
        // Only consider active users that are NOT currently excluded by the UI session
        List<ColleagueRecord> eligiblePool = pool
            .Where(c => c.IsActive && !excludedIds.Contains(c.Id))
            .ToList();
        
        if (eligiblePool.Count == 0)
        {
            return null;
        }

        int minModerationCount = eligiblePool.Min(c => c.ModerationCount);
        
        List<ColleagueRecord> eligibleCandidates = eligiblePool
            .Where(c => c.ModerationCount == minModerationCount)
            .ToList();

        Random random = new();
        int selectedIndex = random.Next(eligibleCandidates.Count);

        return eligibleCandidates[selectedIndex];
    }
}