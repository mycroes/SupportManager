using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;

namespace SupportManager.Web.Pages
{
    public class OverviewModel : PageModel
    {
        private readonly IMediator mediator;

        public OverviewModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.IsObserver() && !User.IsSuperUser()) return Forbid();

            Data = await mediator.Send(new Query());

            return Page();
        }

        public record Query : IRequest<Result>;

        public record Result(List<Result.Team> Teams)
        {
            public record Team
            {
                public int Id { get; init; }
                public string Name { get; init; }
                public string CurrentUser { get; init; }
                public string CurrentPhoneNumber { get; init; }
                public DateTimeOffset? CurrentSince { get; init; }
                public string NextUser { get; init; }
                public DateTimeOffset? NextWhen { get; init; }
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var now = DateTimeOffset.Now;

                var teams = from t in db.Teams
                    where !t.Deleted
                    let current = db.ForwardingStates
                        .Where(f => f.TeamId == t.Id)
                        .OrderByDescending(f => f.When)
                        .FirstOrDefault()
                    let next = db.ScheduledForwards
                        .Where(s => s.TeamId == t.Id && !s.Deleted && s.When > now)
                        .OrderBy(s => s.When)
                        .FirstOrDefault()
                    orderby t.Name
                    select new Result.Team
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CurrentUser = current.DetectedPhoneNumber.User.DisplayName,
                        CurrentPhoneNumber = current.DetectedPhoneNumber.Value ?? current.RawPhoneNumber,
                        CurrentSince = (DateTimeOffset?) current.When,
                        NextUser = next.PhoneNumber.User.DisplayName,
                        NextWhen = (DateTimeOffset?) next.When
                    };

                return new Result(await teams.ToListAsync(cancellationToken));
            }
        }
    }
}
