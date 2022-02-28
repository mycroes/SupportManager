using System.Data.Entity;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class DeleteForwardModel : PageModel
    {
        private readonly IMediator mediator;

        public DeleteForwardModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query) => Data = await mediator.Send(query);

        public async Task<IActionResult> OnPostAsync(int teamId)
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { teamId });
        }

        public record Query(int Id) : IRequest<Command>;

        public record Command : IRequest
        {
            public int Id { get; init; }
            public int TeamId { get; init; }
            public string UserName { get; init; }
            public string PhoneNumber { get; init; }
            public DateTimeOffset When { get; init; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var scheduled = await db.ScheduledForwards.FindAsync(message.Id);
                if (scheduled.ScheduleId != null)
                {
                    BackgroundJob.Delete(scheduled.ScheduleId);
                }
                db.ScheduledForwards.Remove(scheduled);
                await db.SaveChangesAsync();
            }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly SupportManagerContext db;

            public QueryHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Command> Handle(Query query, CancellationToken cancellationToken)
            {
                return await db.ScheduledForwards.Where(s => s.Id == query.Id).Select(s =>
                    new Command
                    {
                        Id = s.Id,
                        TeamId = s.TeamId,
                        PhoneNumber = s.PhoneNumber.Value,
                        UserName = s.PhoneNumber.User.DisplayName,
                        When = s.When
                    }).SingleAsync();
            }
        }
    }
}
