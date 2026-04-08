namespace MeetingApp.Domain.Moderators;

public class ColleagueRecord
{
    public Guid Id { get; init; }
    public string Upn { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public int ModerationCount { get; private set; }
    public bool IsActive { get; private set; }

#pragma warning disable CS8618 
    private ColleagueRecord() { }
#pragma warning restore CS8618

    public ColleagueRecord(
        string upn,
        string firstName, 
        string lastName, 
        int initialModerationCount = 0)
    {
        Id = Guid.NewGuid();
        Upn = upn.ToLowerInvariant().Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DisplayName = $"{FirstName} {LastName}";
        ModerationCount = initialModerationCount;
        IsActive = true;
    }

    public void UpdateProfile(string upn, string firstName, string lastName)
    {
        Upn = upn.ToLowerInvariant().Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DisplayName = $"{FirstName} {LastName}";
    }

    public void IncrementModerationCount() => ModerationCount++;

    // New domain behaviors for the requested features
    public void UndoModerationCount()
    {
        if (ModerationCount > 0) ModerationCount--;
    }

    public void ResetModerationCount() => ModerationCount = 0;

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}