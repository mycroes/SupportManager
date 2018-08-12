using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Home
{
    public static class Index
    {
        public class Query : IRequest<Result>
        {
            public int TeamId { get; set; }
        }

        public class Result
        {
            public int TeamId { get; set; }
            public string TeamName { get; set; }
            public Registration CurrentStatus { get; set; }
            public List<Member> Members { get; set; }
            public List<Registration> Schedule { get; set; }
            public List<Registration> History { get; set; }

            public class Member
            {
                public string DisplayName { get; set; }
                public string PrimaryPhoneNumber { get; set; }
                public int? PrimaryPhoneNumberId { get; set; }
            }

            public class Registration
            {
                public int Id { get; set; }
                public string User { get; set; }
                public string PhoneNumber { get; set; }
                public DateTimeOffset When { get; set; }
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query message, CancellationToken cancellationToken)
            {
                var team = await db.Teams.FindAsync(message.TeamId);
                var members = from t in db.Teams
                    from tm in t.Members
                    let m = tm.User
                    where t.Id == message.TeamId
                    select new Result.Member
                    {
                        DisplayName = m.DisplayName,
                        PrimaryPhoneNumber = m.PrimaryPhoneNumber.Value,
                        PrimaryPhoneNumberId = m.PrimaryPhoneNumber.Id
                    };

                var schedule = from s in db.ScheduledForwards
                    where s.TeamId == message.TeamId
                    where s.When > DateTimeOffset.Now
                    where !s.Deleted
                    orderby s.When
                    select new Result.Registration
                    {
                        Id = s.Id,
                        PhoneNumber = s.PhoneNumber.Value,
                        User = s.PhoneNumber.User.DisplayName,
                        When = s.When
                    };

                var history = from r in db.ForwardingStates
                    where r.TeamId == message.TeamId
                    orderby r.When descending
                    select new Result.Registration
                    {
                        Id = r.Id,
                        PhoneNumber = r.DetectedPhoneNumber.Value ?? r.RawPhoneNumber,
                        User = r.DetectedPhoneNumber.User.DisplayName,
                        When = r.When
                    };

                return new Result
                {
                    TeamId = team.Id,
                    TeamName = team.Name,
                    CurrentStatus = await history.FirstOrDefaultAsync(),
                    Members = await members.ToListAsync(),
                    Schedule = await schedule.Take(10).ToListAsync(),
                    History = await history.Skip(1).Take(10).ToListAsync()
                };
            }
        }
    }
}