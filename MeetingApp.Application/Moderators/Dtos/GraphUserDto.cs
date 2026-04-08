namespace MeetingApp.Application.Moderators.Dtos;

public record GraphUserDto(
    Guid EntraObjectId, 
    string Upn, 
    string FirstName, 
    string LastName, 
    string DisplayName, 
    string Email,
    string? ProfilePictureUri);