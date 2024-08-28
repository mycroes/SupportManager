using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.ScheduleTemplates;

public class EditModel(IMediator mediator) : TeamPageModel
{
    [BindProperty]
    public ViewModel Data { get; set; }

    [BindProperty]
    public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Data = await mediator.Send(new Query<ViewModel>(id));

        return Data == null ? new NotFoundResult() : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await mediator.Send(new Update<ViewModel>(Id, Data));

        return this.RedirectToPageJson(nameof(Index), new { TeamId });
    }

    public class CommandHandler(SupportManagerContext db, TeamId teamId) : AsyncRequestHandler<Update<ViewModel>>
    {
        protected override async Task Handle(Update<ViewModel> request, CancellationToken cancellationToken)
        {
            var (id, model) = request;

            var template = await db.ScheduleTemplates.Where(t => t.TeamId == teamId.Value).Where(t => t.Id == id)
                .Include(t => t.Entries).FirstOrDefaultAsync(cancellationToken);

            if (template is null) throw new InvalidOperationException();

            template.Name = model.Name;
            template.StartDay = model.StartDay;

            foreach (var entry in template.Entries)
            {
                db.ScheduleTemplateEntries.Remove(entry);
            }

            template.Entries = model.Entries.Select(BuildEntry)
                .OrderBy(x => x.DayOfWeek < model.StartDay ? 1 : 0).ThenBy(x => x.DayOfWeek)
                .ThenBy(x => x.Time).ToList();
        }

        private static ScheduleTemplateEntry BuildEntry(ViewModel.Entry input)
        {
            return new ScheduleTemplateEntry
            {
                DayOfWeek = input.DayOfWeek!.Value, Time = input.Time!.Value, UserSlot = input.UserSlot!.Value
            };
        }
    }
}