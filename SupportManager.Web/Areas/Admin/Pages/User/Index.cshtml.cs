using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Areas.Admin.Pages.User
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

        public record Query(int? PageNumber) : IRequest<Result>;

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
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<DAL.User, Result.User>();
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
                var users = await db.Users.OrderBy(u => u.DisplayName)
                    .ProjectToPagedListAsync<Result.User>(mapper.ConfigurationProvider, message.PageNumber ?? 1,
                        Pagination.PageSize);
                return new Result { Users = users, Query = message };
            }
        }
    }
}
