using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class DetailsQueryHandler : IRequestHandler<DetailsQuery, DetailsModel>
    {
        private readonly SupportManagerContext db;
        private readonly IMapper mapper;

        public DetailsQueryHandler(SupportManagerContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<DetailsModel> Handle(DetailsQuery request, CancellationToken cancellationToken)
        {
            return await db.Users.WhereUserLoginIs(request.UserName).ProjectTo<DetailsModel>(mapper.ConfigurationProvider).SingleAsync();
        }
    }
}