using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Pages.Shared
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly SupportManagerContext db;

        public MenuViewComponent(SupportManagerContext db)
        {
            this.db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            MenuModel team = null;
            if (int.TryParse(RouteData.Values["teamId"]?.ToString(), out var teamId))
            {
                var dbTeam = await db.Teams.FindAsync(teamId);
                if (dbTeam != null)
                {
                    team = new MenuModel(dbTeam.Id, dbTeam.Name);
                }
            }

            return View(team ?? new MenuModel(0, ""));
        }
    }
}
