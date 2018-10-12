
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Pages.User.ApiKeys
{
    public class ListModel : PageModel
    {
        private readonly IMediator mediator;

        public ListModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; private set; }

        public async Task OnGetAsync() => Data = await mediator.Send(new Query {UserName = User.Identity.Name});

        public class Result
        {
            public List<ApiKey> ApiKeys { get; set; }

            public class ApiKey
            {
                public int Id { get; set; }
                public string Value { get; set; }
            }
        }

        public class Query : IRequest<Result>
        {
            public string UserName { get; set; }
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

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var apiKeys = await db.Users.WhereUserLoginIs(request.UserName).SelectMany(u => u.ApiKeys)
                    .ProjectToListAsync<Result.ApiKey>(mapper.ConfigurationProvider);

                return new Result {ApiKeys = apiKeys};
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<ApiKey, Result.ApiKey>();
        }
    }
}