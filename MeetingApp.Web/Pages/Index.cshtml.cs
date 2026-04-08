using System.Text.Json;
using MediatR;
using MeetingApp.Application.Moderators.Commands.ManagePool;
using MeetingApp.Application.Moderators.Commands.SelectNextModerator;
using MeetingApp.Application.Moderators.Queries.GetFullPool;
using MeetingApp.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MeetingApp.Web.Pages;

[Authorize]
public class IndexModel(IMediator mediator) : PageModel
{
    // Reusing the new FullPoolQuery to show greyed out users here too
    public List<PoolMemberDto> ActivePool { get; private set; } = [];
    
    public SelectedModeratorDto? SelectedModerator { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Feature 5: Track excluded IDs losslessly across HTTP posts
    [BindProperty]
    public string ExcludedIdsJson { get; set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadActivePoolAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostSelectModeratorAsync(CancellationToken cancellationToken)
    {
        var excludedIds = JsonSerializer.Deserialize<List<Guid>>(ExcludedIdsJson) ?? [];
        
        var result = await mediator.Send(new SelectNextModeratorCommand(excludedIds), cancellationToken);

        if (result.IsSuccess) SelectedModerator = result.Value;
        else ErrorMessage = result.ErrorMessage;

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    // Feature 5: The Reject Flow
    public async Task<IActionResult> OnPostRejectAsync(Guid rejectedId, CancellationToken cancellationToken)
    {
        // 1. Un-do the moderation count physically in the DB
        await mediator.Send(new UndoModerationCommand(rejectedId), cancellationToken);

        // 2. Add the user to the temporary exclusion list
        var excludedIds = JsonSerializer.Deserialize<List<Guid>>(ExcludedIdsJson) ?? [];
        excludedIds.Add(rejectedId);
        ExcludedIdsJson = JsonSerializer.Serialize(excludedIds);

        // 3. Immediately re-roll the dice with the new exclusion list
        var result = await mediator.Send(new SelectNextModeratorCommand(excludedIds), cancellationToken);
        
        if (result.IsSuccess) SelectedModerator = result.Value;
        else ErrorMessage = result.ErrorMessage;

        await LoadActivePoolAsync(cancellationToken);
        return Page();
    }

    // Feature 5: Clearing exclusions
    public IActionResult OnPostResetExclusions()
    {
        ExcludedIdsJson = "[]";
        return RedirectToPage();
    }

    private async Task LoadActivePoolAsync(CancellationToken cancellationToken)
    {
        var fullPool = await mediator.Send(new GetFullPoolQuery(), cancellationToken);
        // Ensure the Index page only displays active users or greys out inactive ones.
        ActivePool = fullPool.OrderByDescending(c => c.IsActive).ToList();
    }
}