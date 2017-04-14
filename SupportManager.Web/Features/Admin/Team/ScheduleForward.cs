using System;
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
            public SupportTeam Team { get; set; }
            public UserPhoneNumber PhoneNumber { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public void Handle(Command message)
            {
                var scheduledForward =
                    new ScheduledForward {Team = message.Team, PhoneNumber = message.PhoneNumber, When = message.When};

                db.ScheduledForwards.Add(scheduledForward);
                db.SaveChanges();

                BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id), message.When);
            }
        }
    }
}