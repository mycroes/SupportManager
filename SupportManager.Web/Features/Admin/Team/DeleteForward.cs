using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
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

        public class Handler : IAsyncRequestHandler<Command>, IAsyncRequestHandler<Request, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task Handle(Command message)
            {
                var scheduled = db.ScheduledForwards.Find(message.Id);
                if (scheduled.ScheduleId != null)
                {
                    BackgroundJob.Delete(scheduled.ScheduleId);
                }
                db.ScheduledForwards.Remove(scheduled);
                await db.SaveChangesAsync();
            }

            public async Task<Result> Handle(Request message)
            {
                return await db.ScheduledForwards.Where(s => s.Id == message.Id)
                    .Select(s => new Result
                    {
                        Id = s.Id,
                        PhoneNumber = s.PhoneNumber.Value,
                        UserName = s.PhoneNumber.User.DisplayName,
                        When = s.When
                    })
                    .SingleAsync();
            }
        }
    }
}