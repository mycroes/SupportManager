using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class DetailsQueryHandler : IRequestHandler<DetailsQuery, DetailsModel>
    {
        private readonly SupportManagerContext db;

        public DetailsQueryHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public async Task<DetailsModel> Handle(DetailsQuery request, CancellationToken cancellationToken)
        {
            return await db.Users.WhereUserLoginIs(request.UserName).ProjectTo<DetailsModel>().SingleAsync();
        }
    }
}