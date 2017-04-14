using System;
using Hangfire;
using MediatR;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Web.Features.Admin.Team
{
    public static class EditForward
    {
        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
        }

        public class Command : IRequest
        {
            public int Id { get; set; }
            public UserPhoneNumber PhoneNumber { get; set; }
            public DateTimeOffset When { get; set; }
        }

        public class Handler : IRequestHandler<Command>, IRequestHandler<Query, Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public Command Handle(Query message)
            {
                var original = db.ScheduledForwards.Find(message.Id);
                return new Command {Id = message.Id, PhoneNumber = original.PhoneNumber, When = original.When};
            }

            public void Handle(Command message)
            {
                var original = db.ScheduledForwards.Find(message.Id);
                original.Deleted = true;

                var scheduledForward =
                    new ScheduledForward { Team = original.Team, PhoneNumber = message.PhoneNumber, When = message.When };

                db.ScheduledForwards.Add(scheduledForward);
                db.SaveChanges();

                BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id), message.When);
            }
        }
    }
}