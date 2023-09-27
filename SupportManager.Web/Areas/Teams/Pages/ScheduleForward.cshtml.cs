using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Contracts;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class ScheduleForwardModel : TeamPageModel
    {
        private readonly IMediator mediator;

        public ScheduleForwardModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public void OnGet() => Data = new Command { TeamId = TeamId };

        public async Task<IActionResult> OnPostAsync()
        {
            if (Data.When <= DateTimeOffset.Now) return new BadRequestResult();

            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { TeamId });
        }

        public record Command : IRequest
        {
            public DateTimeOffset When { get; init; } = DateTimeOffset.Now;
            public int TeamId { get; init; }
            public UserPhoneNumber PhoneNumber { get; init; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var scheduledForward = new ScheduledForward
                {
                    TeamId = request.TeamId, PhoneNumber = request.PhoneNumber, When = request.When
                };

                db.BeginTransaction();
                db.ScheduledForwards.Add(scheduledForward);
                await db.SaveChangesAsync(cancellationToken);

                scheduledForward.ScheduleId =
                    BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id, null),
                        request.When);

                await db.SaveChangesAsync(cancellationToken);
                await db.CommitTransactionAsync();
            }
        }
    }
}
