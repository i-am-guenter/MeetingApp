using MediatR;
using MeetingApp.Application.Moderators.Commands.SelectNextModerator;
using MeetingApp.Application.Moderators.Queries.GetActivePool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MeetingApp.Web.Pages;

[Authorize]
public class IndexModel(IMediator mediator) : PageModel
{
    public List<PoolMemberDto> ActivePool { get; private set; } = [];
    public SelectedModeratorDto? ProposedModerator { get; private set; }
    
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    [BindProperty]
    public Guid? ProposedId { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadActivePoolAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostProposeAsync(CancellationToken cancellationToken)
    {
        // Architectural Fix: Clear ModelState to ensure new IDs are bound correctly to the view
        ModelState.Clear();
        
        // Fix for CS1729: Calling parameterless constructor as rejections are now DB-backed
        var result = await mediator.Send(new ProposeModeratorCommand(), cancellationToken);

        if (result.IsSuccess && result.Value is not null) 
        {
            ProposedModerator = result.Value;
            ProposedId = result.Value.Id; 
        }
        else 
        {
            ErrorMessage = result.ErrorMessage;
            ProposedId = null;
        }

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(CancellationToken cancellationToken)
    {
        if (ProposedId.HasValue)
        {
            var result = await mediator.Send(new CommitModeratorCommand(ProposedId.Value), cancellationToken);

            if (result.IsSuccess)
            {
                SuccessMessage = "Auswahl bestätigt. Der Moderator wurde erfolgreich gespeichert.";
                ProposedId = null;
                ModelState.Clear();
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRejectAsync(CancellationToken cancellationToken)
    {
        if (ProposedId.HasValue)
        {
            // NEW: The rejection is persisted in the database, no more local JSON tracking
            await mediator.Send(new RejectModeratorCommand(ProposedId.Value), cancellationToken);

            // Re-trigger proposal immediately. The logic will now ignore the rejected user based on DB state.
            return await OnPostProposeAsync(cancellationToken);
        }

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostResetFiltersAsync(CancellationToken cancellationToken)
    {
        await mediator.Send(new ResetRejectionsCommand(), cancellationToken);
        ModelState.Clear();
        return RedirectToPage();
    }

    private async Task LoadActivePoolAsync(CancellationToken cancellationToken)
    {
        ActivePool = await mediator.Send(new GetActivePoolQuery(), cancellationToken);
    }
}