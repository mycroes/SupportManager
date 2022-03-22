using System.Data.Entity;
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
                    var memberShip = await db.Entry(dbTeam).Collection(t => t.Members).Query()
                        .Where(m => m.User.Login == User.Identity.Name).FirstOrDefaultAsync();

                    team = new MenuModel(dbTeam.Id, dbTeam.Name, memberShip?.IsAdministrator ?? false);
                }
            }

            return View(team ?? new MenuModel(0, "", false));
        }
    }
}
