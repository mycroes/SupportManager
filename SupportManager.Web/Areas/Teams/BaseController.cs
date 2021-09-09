using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams
{
    [Area("Teams")]
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected int TeamId => int.Parse((string) ControllerContext.RouteData.Values["teamId"]);

        protected SupportTeam Team { get; private set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = (SupportManagerContext) context.HttpContext.RequestServices.GetService(typeof(SupportManagerContext));
            Team = await db.Teams.FindAsync(TeamId);

            if (Team == null)
            {
                context.Result = NotFound();
                return;
            }

            var userName = context.HttpContext.User.Identity.Name;
            if (!await db.TeamMembers.AnyAsync(x => x.TeamId == Team.Id && x.User.Login == userName))
            {
                context.Result = NotFound();
                return;
            }

            ViewData["TeamName"] = Team.Name;

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
