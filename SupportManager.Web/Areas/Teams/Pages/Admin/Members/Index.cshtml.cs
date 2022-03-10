using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.Members
{
    public class IndexModel : TeamPageModel
    {
        private readonly IMediator mediator;

        public IndexModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; set; }

        public async Task OnGetAsync(Query query)
        {
            Data = await mediator.Send(query);
        }

        public record Query(int TeamId, int? PageNumber) : IRequest<Result>;

        public record Result
        {
            public IPagedList<User> Users { get; init; }

            public Query Query { get; init; }

            public class User
            {
                public int Id { get; init; }
                public string DisplayName { get; init; }
                public string Login { get; init; }
                public string PrimaryPhoneNumberValue { get; init; }
                public bool IsAdministrator { get; set; }
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<User, Result.User>();
                CreateProjection<TeamMember, Result.User>().IncludeMembers(tm => tm.User);
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public Handler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<Result> Handle(Query message, CancellationToken cancellationToken)
            {
                var users = await db.Teams.Where(t => t.Id == message.TeamId).SelectMany(t => t.Members)
                    .OrderBy(tm => tm.User.DisplayName)
                    .ProjectToPagedListAsync<Result.User>(mapper.ConfigurationProvider, message.PageNumber ?? 1,
                        Pagination.PageSize);
                return new Result { Users = users, Query = message };
            }
        }
    }
}
