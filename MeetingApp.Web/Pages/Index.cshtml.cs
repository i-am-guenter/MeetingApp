using MediatR;
using MeetingApp.Application.Moderators.Commands.SelectNextModerator;
using MeetingApp.Application.Moderators.Queries.GetActivePool;
using MeetingApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MeetingApp.Web.Pages;

// Enforces Entra ID authentication for this entire page.
[Authorize]
public class IndexModel(IMediator mediator) : PageModel
{
    // The Read-Model for our UI grid
    public List<PoolMemberDto> ActivePool { get; private set; } = [];
    
    // The Command-Result properties
    public SelectedModeratorDto? SelectedModerator { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Hydrate the data grid on initial load via CQRS Query
        await LoadActivePoolAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostSelectModeratorAsync(CancellationToken cancellationToken)
    {
        var command = new SelectNextModeratorCommand();
        
        Result<SelectedModeratorDto> result = await mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            SelectedModerator = result.Value;
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }

        // Re-hydrate the grid after a selection to show the incremented counts
        await LoadActivePoolAsync(cancellationToken);
        
        return Page();
    }

    private async Task LoadActivePoolAsync(CancellationToken cancellationToken)
    {
        var query = new GetActivePoolQuery();
        ActivePool = await mediator.Send(query, cancellationToken);
    }
}