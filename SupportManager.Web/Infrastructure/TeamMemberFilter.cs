using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure;

public class TeamMemberFilter : IAsyncPageFilter
{
    private readonly SupportManagerContext db;

    public TeamMemberFilter(SupportManagerContext db)
    {
        this.db = db;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!int.TryParse(context.RouteData.Values["teamId"]?.ToString(), out var teamId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        if (!context.HttpContext.User.HasClaim(SupportManagerClaimTypes.TeamMember, teamId.ToString()))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next.Invoke();
    }
}