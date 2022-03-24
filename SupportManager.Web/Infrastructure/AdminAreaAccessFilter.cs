using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SupportManager.Web.Infrastructure;

internal class AdminAreaAccessFilter : IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.ActionDescriptor.AreaName != "Admin") return;

        if (context.HttpContext.User.HasClaim(SupportManagerClaimTypes.SuperUser, true.ToString())) return;

        context.Result = new ForbidResult();
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }
}