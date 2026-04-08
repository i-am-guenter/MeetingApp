using MediatR;
using MeetingApp.Application.Moderators.Commands.ManagePool;
using MeetingApp.Application.Moderators.Queries.GetActivePool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace MeetingApp.Web.Pages;

// Enforces Entra ID authentication for accessing the management area
[Authorize]
public class ManagePoolModel(IMediator mediator) : PageModel
{
    // Read-Model for the UI Grid
    public List<PoolMemberDto> ActivePool { get; private set; } = [];
    
    // Command-Models for Data Entry (Two-Way Binding)
    [BindProperty]
    [Required(ErrorMessage = "The UPN is strictly required.")]
    [EmailAddress(ErrorMessage = "The UPN must be a valid email format.")]
    public string InputUpn { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "First name is required.")]
    public string InputFirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Last name is required.")]
    public string InputLastName { get; set; } = string.Empty;

    // UI Feedback Properties
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadActivePoolAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Validation failed. Please provide valid data for all required fields.";
            await LoadActivePoolAsync(cancellationToken);
            return Page();
        }

        // Dispatching the local CRUD command. Hydration via Graph API is intentionally bypassed.
        var command = new AddColleagueCommand(InputUpn, InputFirstName, InputLastName);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            SuccessMessage = $"Colleague '{InputFirstName} {InputLastName}' was successfully added to the pool.";
            
            // Clear the form after a successful insert to prevent accidental double-submits
            ModelState.Clear();
            InputUpn = string.Empty;
            InputFirstName = string.Empty;
            InputLastName = string.Empty;
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleAsync(string upn, CancellationToken cancellationToken)
    {
        // Architectural constraint: We use the UPN to target the user, 
        // as we aligned our commands to rely on the UPN instead of Guids for easier UI integration.
        var command = new ToggleColleagueStatusCommand(upn);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            SuccessMessage = $"Status for UPN '{upn}' was successfully toggled.";
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    private async Task LoadActivePoolAsync(CancellationToken cancellationToken)
    {
        var query = new GetActivePoolQuery();
        ActivePool = await mediator.Send(query, cancellationToken);
    }
}