using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.Members
{
    public class ListModel : PageModel
    {
        private readonly IMediator mediator;

        public ListModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; private set; }

        public async Task OnGetAsync(Query query) => Data = await mediator.Send(query);

        public class Query : IRequest<Result>
        {
            public int TeamId { get; set; }
            public int? PageNumber { get; set; }
        }

        public class Result
        {
            public IPagedList<User> Users { get; set; }

            public class User
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public string Login { get; set; }
                public string PrimaryPhoneNumberValue { get; set; }
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
                var users = await db.Teams.Where(t => t.Id == message.TeamId)
                    .SelectMany(t => t.Members.Select(m => m.User)).OrderBy(u => u.DisplayName)
                    .ProjectToPagedListAsync<Result.User>(mapper.ConfigurationProvider, message.PageNumber ?? 1,
                        Pagination.PageSize);
                return new Result {Users = users};
            }
        }
    }
}
