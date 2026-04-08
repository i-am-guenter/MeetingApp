using Azure.Identity;
using MeetingApp.Application.Moderators.Dtos;
using MeetingApp.Application.Moderators.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace MeetingApp.Infrastructure.Identity;

/// <summary>
/// Infrastructure implementation of the Microsoft Graph integration.
/// Adjusted for strict enterprise security: Uses direct UPN lookups to comply with User.ReadBasic.All restrictions.
/// </summary>
public class EntraIdGraphService : IGraphService
{
    private readonly GraphServiceClient graphClient;

    public EntraIdGraphService(IConfiguration configuration)
    {
        string? tenantId = configuration["AzureAd:TenantId"];
        string? clientId = configuration["AzureAd:ClientId"];
        string? clientSecret = configuration["AzureAd:ClientSecret"];

        // ClientSecretCredential operating with User.ReadBasic.All Application Permission.
        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        
        graphClient = new GraphServiceClient(clientSecretCredential, ["https://graph.microsoft.com/.default"]);
    }

    public async Task<GraphUserDto?> GetUserAsync(Guid entraObjectId, CancellationToken cancellationToken = default)
    {
        return await FetchUserByIdentifierAsync(entraObjectId.ToString(), cancellationToken);
    }

    public Task<List<GraphUserDto>> GetUsersByDepartmentsAsync(string[] departments, CancellationToken cancellationToken = default)
    {
        if (departments.Length == 0) return Task.FromResult(new List<GraphUserDto>());

        // Architectural Guardrail: 
        // Our enterprise security policy restricts us to User.ReadBasic.All. 
        // Querying by department requires Advanced Query execution which mandates User.Read.All.
        throw new NotSupportedException("Security Policy Violation: Department syncing is disabled. Please configure specific UPNs in the appsettings.json instead.");
    }

    public async Task<List<GraphUserDto>> GetUsersByUpnsAsync(string[] upns, CancellationToken cancellationToken = default)
    {
        if (upns.Length == 0) return [];

        // Concurrent fetching pattern:
        // By fetching users individually via their UPN, we perform a direct object lookup.
        // This completely bypasses the Graph API's search/filter engine and perfectly complies 
        // with the restricted User.ReadBasic.All Application Permission.
        var fetchTasks = upns.Select(upn => FetchUserByIdentifierAsync(upn, cancellationToken));
        
        var results = await Task.WhenAll(fetchTasks);

        return results
            .Where(u => u != null)
            .Cast<GraphUserDto>()
            .ToList();
    }

    /// <summary>
    /// Core helper to fetch a user directly by their Graph identifier (can be an Object ID GUID or a UPN string).
    /// </summary>
    private async Task<GraphUserDto?> FetchUserByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            var user = await graphClient.Users[identifier]
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "userPrincipalName", "givenName", "surname", "displayName", "mail"];
                }, cancellationToken);

            if (user is null) return null;

            return MapToDto(user);
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            // Graceful handling if a UPN configured in appsettings.json does not exist in Entra ID.
            return null;
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError oDataError)
        {
            string errorCode = oDataError.Error?.Code ?? "UnknownErrorCode";
            string errorMessage = oDataError.Error?.Message ?? "No error message provided by Entra ID";
            
            throw new InvalidOperationException($"Microsoft Graph API Direct Fetch Error: [{errorCode}] {errorMessage} | Identifier: {identifier}", oDataError);
        }
    }

    private static GraphUserDto MapToDto(Microsoft.Graph.Models.User user)
    {
        return new GraphUserDto(
            Guid.Parse(user.Id!),
            user.UserPrincipalName ?? string.Empty,
            user.GivenName ?? string.Empty,
            user.Surname ?? string.Empty,
            user.DisplayName ?? string.Empty,
            user.Mail ?? user.UserPrincipalName ?? string.Empty,
            null
        );
    }
}


/*
using Azure.Identity;
using MeetingApp.Application.Moderators.Dtos;
using MeetingApp.Application.Moderators.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace MeetingApp.Infrastructure.Identity;

/// <summary>
/// Infrastructure implementation of the Microsoft Graph integration.
/// Uses the Client Credentials Flow (Daemon) to authenticate and query Entra ID.
/// </summary>
public class EntraIdGraphService : IGraphService
{
    private readonly GraphServiceClient graphClient;

    public EntraIdGraphService(IConfiguration configuration)
    {
        string? tenantId = configuration["AzureAd:TenantId"];
        string? clientId = configuration["AzureAd:ClientId"];
        string? clientSecret = configuration["AzureAd:ClientSecret"];

        // ClientSecretCredential requires Application Permissions (e.g., User.ReadBasic.All) 
        // granted and admin-consented in the Entra ID App Registration.
        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        
        graphClient = new GraphServiceClient(clientSecretCredential, ["https://graph.microsoft.com/.default"]);
    }

    public async Task<GraphUserDto?> GetUserAsync(Guid entraObjectId, CancellationToken cancellationToken = default)
    {
        var user = await graphClient.Users[entraObjectId.ToString()]
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = ["id", "userPrincipalName", "givenName", "surname", "displayName", "mail"];
            }, cancellationToken);

        if (user is null) return null;

        return MapToDto(user);
    }

    // Retained for backward compatibility with older handlers during transition
    public async Task<List<GraphUserDto>> GetUsersByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await GetUsersByDepartmentsAsync([department], cancellationToken);
    }

    public async Task<List<GraphUserDto>> GetUsersByDepartmentsAsync(string[] departments, CancellationToken cancellationToken = default)
    {
        if (departments.Length == 0) return [];

        var filterClauses = departments.Select(d => $"department eq '{d.Replace("'", "''")}'");
        string filterString = string.Join(" or ", filterClauses);

        return await FetchUsersWithFilterAsync(filterString, cancellationToken);
    }

    public async Task<List<GraphUserDto>> GetUsersByUpnsAsync(string[] upns, CancellationToken cancellationToken = default)
    {
        if (upns.Length == 0) return [];

        var filterClauses = upns.Select(u => $"userPrincipalName eq '{u.Replace("'", "''")}'");
        string filterString = string.Join(" or ", filterClauses);

        return await FetchUsersWithFilterAsync(filterString, cancellationToken);
    }

    // ... existing code ...
    private async Task<List<GraphUserDto>> FetchUsersWithFilterAsync(string filterString, CancellationToken cancellationToken)
    {
        var users = new List<GraphUserDto>();

        try
        {
            var response = await graphClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = filterString;
                    // Preemptive Fix: Added "department" to the Select array. 
                    // Graph API sometimes rejects queries if you filter on an attribute that is not projected.
                    requestConfiguration.QueryParameters.Select = ["id", "userPrincipalName", "givenName", "surname", "displayName", "mail", "department"];
                    
                    // Architectural Fix: Enable Advanced Queries for Entra ID
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    requestConfiguration.QueryParameters.Count = true;
                }, cancellationToken);

            if (response?.Value != null)
            {
                // Graph API results are paginated. We must use a PageIterator to fetch all users 
                // if the payload exceeds the default page size.
                var pageIterator = Microsoft.Graph.PageIterator<Microsoft.Graph.Models.User, Microsoft.Graph.Models.UserCollectionResponse>
                    .CreatePageIterator(graphClient, response, user =>
                    {
                        users.Add(MapToDto(user));
                        return true;
                    });

                await pageIterator.IterateAsync(cancellationToken);
            }
        }
        catch (Microsoft.Graph.Models.ODataErrors.ODataError oDataError)
        {
            // Architectural Diagnostic: We MUST unwrap the generic ODataError to see what Entra ID is actually complaining about.
            string errorCode = oDataError.Error?.Code ?? "UnknownErrorCode";
            string errorMessage = oDataError.Error?.Message ?? "No error message provided by Entra ID";
            
            // Re-throwing with the exact Microsoft Graph error details exposed to the console/logger
            throw new InvalidOperationException(
                $"Microsoft Graph API Error: [{errorCode}] {errorMessage} | Filter used: {filterString}", 
                oDataError);
        }

        return users;
    }

    private static GraphUserDto MapToDto(Microsoft.Graph.Models.User user)
{
    return new GraphUserDto(
        Guid.Parse(user.Id!),
        user.UserPrincipalName ?? string.Empty,
        user.GivenName ?? string.Empty,
        user.Surname ?? string.Empty,
        user.DisplayName ?? string.Empty,
        user.Mail ?? user.UserPrincipalName ?? string.Empty,
        null
    );
}
}
*/