using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SupportManager.Web.Infrastructure;

public class TeamMemberFilter : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.ActionDescriptor.AreaName != "Teams") return;

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

        if (context.ActionDescriptor.ViewEnginePath.StartsWith("/Admin") &&
            !context.HttpContext.User.HasClaim(SupportManagerClaimTypes.TeamAdmin, teamId.ToString()))
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}