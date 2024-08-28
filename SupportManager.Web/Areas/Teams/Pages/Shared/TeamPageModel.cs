using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SupportManager.Web.Areas.Teams.Pages.Shared;

public class TeamPageModel : PageModel
{
    public int TeamId { get; set; }

    public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        int.TryParse(context.RouteData.Values["teamId"]?.ToString(), out var teamId);
        TeamId = teamId;

        var teamIdContainer = context.HttpContext.RequestServices.GetRequiredService<TeamId>();
        teamIdContainer.Value = teamId;

        base.OnPageHandlerSelected(context);
    }
}