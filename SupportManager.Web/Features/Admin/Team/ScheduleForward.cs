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

        public class Handler : IAsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task Handle(Command message)
            {
                var scheduledForward =
                    new ScheduledForward {TeamId = message.TeamId, PhoneNumber = message.PhoneNumber, When = message.When};

                db.BeginTransaction();
                db.ScheduledForwards.Add(scheduledForward);
                await db.SaveChangesAsync();

                scheduledForward.ScheduleId =
                    BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id), message.When);

                await db.SaveChangesAsync();
                db.CloseTransaction();
            }
        }
    }
}