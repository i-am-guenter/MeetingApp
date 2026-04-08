namespace MeetingApp.Application.Configuration;

public class MeetingSettings
{
    public const string SectionName = "MeetingSettings";
    
    public string[] IncludedDepartments { get; init; } = [];
    public string[] IncludedUpns { get; init; } = [];
    public string[] ExcludedUpns { get; init; } = [];
}