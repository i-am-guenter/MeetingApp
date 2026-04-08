namespace MeetingApp.Domain.Moderators;

public static class ModeratorSelectionPolicy
{
    public static ColleagueRecord? SelectNext(IEnumerable<ColleagueRecord> pool)
    {
        List<ColleagueRecord> colleagues = [.. pool];
        
        if (colleagues.Count == 0)
        {
            return null;
        }

        int minModerationCount = colleagues.Min(c => c.ModerationCount);
        
        List<ColleagueRecord> eligibleCandidates = colleagues
            .Where(c => c.ModerationCount == minModerationCount)
            .ToList();

        Random random = new();
        int selectedIndex = random.Next(eligibleCandidates.Count);

        return eligibleCandidates[selectedIndex];
    }
}