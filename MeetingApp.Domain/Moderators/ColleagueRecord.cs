namespace MeetingApp.Domain.Moderators;

public class ColleagueRecord
{
    // Represents the Entra ID Object ID
    public Guid EntraObjectId { get; init; }
    
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public string Email { get; private set; }
    public string? ProfilePictureUri { get; private set; }
    
    // Required locally to filter the pool without querying Graph API for the whole company
    public string Department { get; private set; }
    
    public int ModerationCount { get; private set; }
    public bool IsManuallyAdded { get; init; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// EF Core required parameterless constructor for database materialization.
    /// Marked as private to prevent accidental usage in the Application layer,
    /// enforcing Domain-Driven Design invariants through the rich constructor.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. EF Core handles this.
    private ColleagueRecord() { }
#pragma warning restore CS8618

    public ColleagueRecord(
        Guid entraObjectId, 
        string firstName, 
        string lastName, 
        string displayName, 
        string email, 
        string department, 
        bool isManuallyAdded, 
        int initialModerationCount = 0,
        string? profilePictureUri = null)
    {
        EntraObjectId = entraObjectId;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        Email = email;
        Department = department;
        IsManuallyAdded = isManuallyAdded;
        ModerationCount = initialModerationCount;
        ProfilePictureUri = profilePictureUri;
        IsActive = true;
    }

    public void UpdateProfile(string firstName, string lastName, string displayName, string email, string department, string? profilePictureUri)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName;
        Email = email;
        Department = department;
        ProfilePictureUri = profilePictureUri;
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