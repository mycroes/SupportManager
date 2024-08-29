using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.ScheduleTemplates;

public class DeleteModel(IMediator mediator) : TeamPageModel
{
    public int Id { get; set; }

    public ViewModel Data { get; set; }

    public async Task<IActionResult> OnGetAsync(Query<ViewModel> query)
    {
        Id = query.Id;
        Data = await mediator.Send(query);

        return Data == null ? new NotFoundResult() : Page();
    }

    public async Task<IActionResult> OnPostAsync(Delete<ViewModel> command)
    {
        await mediator.Send(command);

        return this.RedirectToPageJson(nameof(Index), new { TeamId });
    }

    public class CommandHandler(SupportManagerContext db, TeamId teamId) : AsyncRequestHandler<Delete<ViewModel>>
    {
        protected override async Task Handle(Delete<ViewModel> request, CancellationToken cancellationToken)
        {
            var template = await db.ScheduleTemplates.Where(t => t.TeamId == teamId.Value)
                .Where(t => t.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                throw new InvalidOperationException();
            }

            db.ScheduleTemplates.Remove(template);
        }
    }
}