using System.Data.Entity;
using HtmlTags;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages.Shared;

public class ScheduleTemplateListViewComponent(SupportManagerContext db, TeamId teamId) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(Func<ViewModel, IHtmlContent> render)
    {
        var templates = await db.ScheduleTemplates.Where(t => t.TeamId == teamId.Value).OrderBy(t => t.Name)
            .Select(t => new ViewModel(t.Id, t.Name)).WithCtorMembers().ToListAsync();

        return View(new Model(render, templates));
    }

    public record Model(Func<ViewModel, IHtmlContent> Render, IEnumerable<ViewModel> Templates);

    public record ViewModel(int Id, string Name);
}