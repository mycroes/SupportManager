using System;
using System.Threading.Tasks;
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

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task HandleCore(Command request)
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

        public class QueryHandler : AsyncRequestHandler<Query, Command>
        {
            private readonly SupportManagerContext db;

            public QueryHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task<Command> HandleCore(Query message)
            {
                var original = await db.ScheduledForwards.FindAsync(message.Id);
                return new Command {Id = message.Id, PhoneNumber = original.PhoneNumber, When = original.When};
            }
        }
    }
}