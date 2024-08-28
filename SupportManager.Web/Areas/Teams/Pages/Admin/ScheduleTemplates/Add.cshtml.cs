using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.ScheduleTemplates;

public class AddModel(IMediator mediator) : TeamPageModel
{
    [BindProperty]
    public ViewModel Data { get; set; }

    public void OnGet() => Data = new ViewModel(TeamId, null, DayOfWeek.Monday, [new ViewModel.Entry(null, null, null)]);

    public async Task<IActionResult> OnPostAsync()
    {
        await mediator.Send(new Create<ViewModel>(Data));

        return this.RedirectToPageJson(nameof(Index), new { TeamId });
    }

    public class Handler(SupportManagerContext db) : RequestHandler<Create<ViewModel>>
    {
        protected override void Handle(Create<ViewModel> request)
        {
            var model = request.Model;

            var template = new ScheduleTemplate
            {
                TeamId = model.TeamId,
                Name = model.Name,
                StartDay = model.StartDay,
                Entries = model.Entries.Select(BuildEntry)
                    .OrderBy(x => x.DayOfWeek < model.StartDay ? 1 : 0).ThenBy(x => x.DayOfWeek)
                    .ThenBy(x => x.Time).ToList()
            };

            db.ScheduleTemplates.Add(template);
        }

        private static ScheduleTemplateEntry BuildEntry(ViewModel.Entry input)
        {
            return new ScheduleTemplateEntry
            {
                DayOfWeek = input.DayOfWeek!.Value,
                Time = input.Time!.Value,
                UserSlot = input.UserSlot!.Value
            };
        }
    }
}