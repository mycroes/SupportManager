using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.Admin.Team
{
    public static class DeleteForward
    {
        public class Request : IRequest<Result>
        {
            public int Id { get; set; }
        }

        public class Result
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string PhoneNumber { get; set; }
            public DateTimeOffset When { get; set; }
        }

        public class Command : IRequest
        {
            public int Id { get; set; }
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

        public class RequestHandler : IRequestHandler<Request, Result>
        {
            private readonly SupportManagerContext db;

            public RequestHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                return await db.ScheduledForwards.Where(s => s.Id == request.Id).Select(s =>
                    new Result
                    {
                        Id = s.Id,
                        PhoneNumber = s.PhoneNumber.Value,
                        UserName = s.PhoneNumber.User.DisplayName,
                        When = s.When
                    }).SingleAsync();
            }
        }

    }
}