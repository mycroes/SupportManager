using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class FullScheduleModel : TeamPageModel
    {
        private IMediator mediator;

        public FullScheduleModel(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Result Data { get; set; }

        public async Task OnGetAsync(Query query) => Data = await mediator.Send(query);

        public record Query(int TeamId, int? PageNumber) : IRequest<Result>;

        public record Result(Query Query, IPagedList<Registration> Schedule);

        public class Registration
        {
            public int Id { get; set; }
            public string User { get; set; }
            public string PhoneNumber { get; set; }
            public DateTimeOffset When { get; set; }
        }

        internal class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                return new Result(request,
                    await db.ScheduledForwards.Where(s => s.TeamId == request.TeamId)
                        .Where(s => s.When > DateTimeOffset.Now).Where(s => !s.Deleted).OrderBy(s => s.When).Select(s =>
                            new Registration
                            {
                                Id = s.Id,
                                PhoneNumber = s.PhoneNumber.Value,
                                User = s.PhoneNumber.User.DisplayName,
                                When = s.When
                            }).ToPagedListAsync(request.PageNumber ?? 1, Pagination.PageSize));
            }
        }
    }
}