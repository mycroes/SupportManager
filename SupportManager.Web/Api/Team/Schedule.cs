using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using MediatR;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Web.Api.Team
{
    public class Schedule
    {
        public class Query : IRequest<List<ForwardRegistration>>
        {
            public Query(int teamId) => TeamId = teamId;

            public int TeamId { get; }
        }

        public class QueryHandler : IRequestHandler<Query, List<ForwardRegistration>>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public QueryHandler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<List<ForwardRegistration>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await db.ScheduledForwards.Where(fwd => fwd.When >= DateTimeOffset.Now).Where(fwd => fwd.TeamId == request.TeamId).OrderBy(fwd => fwd.When)
                    .Take(10).Select(fwd => new ForwardRegistration
                    {
                        Id = fwd.Id,
                        PhoneNumber = new PhoneNumber
                        {
                            Id = fwd.PhoneNumber.Id,
                            Label = fwd.PhoneNumber.Label,
                            Value = fwd.PhoneNumber.Value
                        },
                        User = new SupportManager.Api.Users.User
                        {
                            Id = fwd.PhoneNumber.User.Id,
                            DisplayName = fwd.PhoneNumber.User.DisplayName
                        },
                        When = fwd.When
                    }).ToListAsync(cancellationToken);
            }
        }

        public class Command : IRequest
        {
            public Command(int teamId, int phoneNumberId, DateTimeOffset @when)
            {
                TeamId = teamId;
                PhoneNumberId = phoneNumberId;
                When = when;
            }

            public int TeamId { get; }
            public int PhoneNumberId { get; }
            public DateTimeOffset When { get; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db) => this.db = db;

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var scheduledForward = new ScheduledForward
                {
                    TeamId = request.TeamId, PhoneNumberId = request.PhoneNumberId, When = request.When
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
