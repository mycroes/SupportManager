using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.Contracts;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class EditForwardModel : PageModel
    {
        private readonly IMediator mediator;

        public EditForwardModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query request) => Data = await mediator.Send(request);

        public async Task<IActionResult> OnPostAsync(int teamId)
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { teamId });
        }

        public record Query(int Id) : IRequest<Command>;

        public class Command : IRequest
        {
            public int Id { get; init; }
            public UserPhoneNumber PhoneNumber { get; init; }
            public DateTimeOffset When { get; init; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command request, CancellationToken canellationToken)
            {
                var original = db.ScheduledForwards.Find(request.Id);

                if (original.ScheduleId != null)
                {
                    BackgroundJob.Delete(original.ScheduleId);
                    original.ScheduleId = null;
                    await db.SaveChangesAsync();
                }

                original.PhoneNumber = request.PhoneNumber;
                original.When = request.When;
                await db.SaveChangesAsync();

                original.ScheduleId =
                    BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(original.Id, null), request.When);
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

            public async Task<Command> Handle(Query message, CancellationToken cancellationToken)
            {
                var original = await db.ScheduledForwards.FindAsync(message.Id);
                return new Command {Id = message.Id, PhoneNumber = original.PhoneNumber, When = original.When};
            }
        }
    }
}
