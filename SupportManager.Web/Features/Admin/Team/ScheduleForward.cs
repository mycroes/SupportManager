using System;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Web.Features.Admin.Team
{
    public static class ScheduleForward
    {
        public class Command : IRequest
        {
            public DateTimeOffset When { get; set; }
            public int TeamId { get; set; }
            public UserPhoneNumber PhoneNumber { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task HandleCore(Command request)
            {
                var scheduledForward = new ScheduledForward
                {
                    TeamId = request.TeamId,
                    PhoneNumber = request.PhoneNumber,
                    When = request.When
                };

                db.BeginTransaction();
                db.ScheduledForwards.Add(scheduledForward);
                await db.SaveChangesAsync();

                scheduledForward.ScheduleId =
                    BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id, null),
                        request.When);

                await db.SaveChangesAsync();
                await db.CommitTransactionAsync();
            }
        }
    }
}