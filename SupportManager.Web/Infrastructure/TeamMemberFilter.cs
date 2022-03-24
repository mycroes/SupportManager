using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SupportManager.Web.Infrastructure;

internal class TeamMemberFilter : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.ActionDescriptor.AreaName != "Teams") return;

        if (context.RouteData.Values["teamId"] is not string teamId)
        {
            context.Result = new BadRequestResult();
            return;
        }

        if (!context.HttpContext.User.HasClaim(SupportManagerClaimTypes.TeamMember, teamId))
        {
            context.Result = new NotFoundResult();
            return;
        }

        if (context.ActionDescriptor.ViewEnginePath.StartsWith("/Admin") &&
            !context.HttpContext.User.HasClaim(SupportManagerClaimTypes.TeamAdmin, teamId))
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}