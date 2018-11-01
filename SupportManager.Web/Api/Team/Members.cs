using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.Api.Users;
using SupportManager.DAL;

namespace SupportManager.Web.Api.Team
{
    public static class Members
    {
        public class Query : IRequest<List<UserDetails>>
        {
            public int TeamId { get; }

            public Query(int teamId) => TeamId = teamId;
        }

        public class Handler : IRequestHandler<Query, List<UserDetails>>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public Handler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<List<UserDetails>> Handle(Query request, CancellationToken cancellationToken) =>
                await db.TeamMembers.Where(m => m.TeamId == request.TeamId).Select(m => m.User)
                    .ProjectToListAsync<UserDetails>(mapper.ConfigurationProvider);
        }
    }
}