using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Home
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