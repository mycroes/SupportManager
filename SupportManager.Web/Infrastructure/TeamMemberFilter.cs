using System.Reflection;
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

        var user = context.HttpContext.User;

        // Permit superuser access to all teams
        if (user.IsSuperUser()) return;

        if (context.RouteData.Values["teamId"] is not string teamId)
        {
            context.Result = new BadRequestResult();
            return;
        }

        if (!user.HasClaim(SupportManagerClaimTypes.TeamMember, teamId))
        {
            if (!user.IsObserver())
            {
                context.Result = new NotFoundResult();
                return;
            }

            // Observers have read-only access to pages that explicitly allow it
            if (!IsObserverAllowed(context))
            {
                context.Result = new ForbidResult();
            }

            return;
        }

        if (context.ActionDescriptor.ViewEnginePath.StartsWith("/Admin") &&
            !user.HasClaim(SupportManagerClaimTypes.TeamAdmin, teamId))
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    private static bool IsObserverAllowed(PageHandlerExecutingContext context)
    {
        var method = context.HttpContext.Request.Method;
        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method)) return false;

        return context.ActionDescriptor.HandlerTypeInfo?.IsDefined(typeof(AllowObserverAttribute), false) ?? false;
    }
}
