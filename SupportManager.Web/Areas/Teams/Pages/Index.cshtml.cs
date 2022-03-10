using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator mediator;

        public IndexModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; set; }

        public async Task OnGetAsync(Query query)
        {
            Data = await mediator.Send(query);
        }

        public record Query(int TeamId) : IRequest<Result>;

        public record Result
        {
            public int TeamId { get; init; }
            public string TeamName { get; init; }
            public Registration CurrentStatus { get; init; }
            public List<Member> Members { get; init; }
            public List<Registration> Schedule { get; init; }
            public int ScheduledCount { get; init; }
            public List<Registration> History { get; init; }

            public record Member
            {
                public string DisplayName { get; init; }
                public string PrimaryPhoneNumber { get; init; }
                public int? PrimaryPhoneNumberId { get; init; }
            }

            public record Registration
            {
                public int Id { get; init; }
                public string User { get; init; }
                public string PhoneNumber { get; init; }
                public DateTimeOffset When { get; init; }
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
                    ScheduledCount = await schedule.CountAsync(),
                    History = await history.Skip(1).Take(10).ToListAsync()
                };
            }
        }
    }
}
