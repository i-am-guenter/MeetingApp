namespace MeetingApp.Domain.Moderators;

/// <summary>
/// The core domain entity representing a pool member.
/// Architectural Update: Decoupled from Entra ID ObjectIds. 
/// Now utilizes a standard Guid as the primary key and the UPN (Email) as the business identifier.
/// </summary>
public class ColleagueRecord
{
    public Guid Id { get; init; }
    
    public string Upn { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    
    public int ModerationCount { get; private set; }
    public bool IsActive { get; private set; }

#pragma warning disable CS8618 // Required for Entity Framework Core
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

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DisplayName = $"{FirstName} {LastName}";
    }

    public void IncrementModerationCount()
    {
        ModerationCount++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}