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

        public class Handler : IAsyncRequestHandler<Command>, IRequestHandler<Query, Command>
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

            public async Task Handle(Command message)
            {
                var original = db.ScheduledForwards.Find(message.Id);

                if (original.ScheduleId != null)
                {
                    BackgroundJob.Delete(original.ScheduleId);
                    original.ScheduleId = null;
                    await db.SaveChangesAsync();
                }

                original.PhoneNumber = message.PhoneNumber;
                original.When = message.When;
                await db.SaveChangesAsync();

                original.ScheduleId = BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(original.Id, null), message.When);
                await db.SaveChangesAsync();
            }
        }
    }
}