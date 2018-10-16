using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.Api.Users;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Api.User
{
    public static class MyDetails
    {
        public class Query : IRequest<UserDetails>
        {
            public Query(string userName) => UserName = userName;

            public string UserName { get; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<DAL.User, UserDetails>();
                CreateMap<UserEmailAddress, EmailAddress>();
                CreateMap<UserPhoneNumber, PhoneNumber>();
            }
        }

        public class Handler : IRequestHandler<Query, UserDetails>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public Handler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<UserDetails> Handle(Query request, CancellationToken cancellationToken) =>
                await db.Users.WhereUserLoginIs(request.UserName)
                    .ProjectToSingleAsync<UserDetails>(mapper.ConfigurationProvider);
        }
    }
}
