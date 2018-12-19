using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.DAL;

namespace SupportManager.Web.Api.Team
{
    public static class Status
    {
        public class Query : IRequest<TeamStatus>
        {
            public int TeamId { get; }

            public Query(int teamId) => TeamId = teamId;
        }

        public class Handler : IRequestHandler<Query, TeamStatus>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<TeamStatus> Handle(Query request, CancellationToken cancellationToken)
            {
                return new TeamStatus
                {
                    CurrentForward =
                        await db.ForwardingStates.Where(s => s.TeamId == request.TeamId).Select(s =>
                            new ForwardRegistration
                            {
                                PhoneNumber =
                                    new PhoneNumber
                                    {
                                        Label = s.DetectedPhoneNumber.Label,
                                        Value = s.DetectedPhoneNumber.Value
                                    },
                                When = s.When,
                                User = new SupportManager.Api.Users.User
                                {
                                    DisplayName = s.DetectedPhoneNumber.User.DisplayName,
                                    Id = s.DetectedPhoneNumber.UserId
                                }
                            }).OrderByDescending(s => s.When).FirstOrDefaultAsync(),
                    ScheduledForward = await db.ScheduledForwards
                        .Where(s => s.TeamId == request.TeamId && s.When > DateTimeOffset.Now).Select(s =>
                            new ForwardRegistration
                            {
                                PhoneNumber =
                                    new PhoneNumber {Label = s.PhoneNumber.Label, Value = s.PhoneNumber.Value},
                                When = s.When,
                                User = new SupportManager.Api.Users.User
                                {
                                    DisplayName = s.PhoneNumber.User.DisplayName, Id = s.PhoneNumber.UserId
                                }
                            }).OrderBy(s => s.When).FirstOrDefaultAsync()
                };
            }
        }
    }
}
