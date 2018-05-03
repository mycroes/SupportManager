using System.Data.Entity;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class DetailsQueryHandler : AsyncRequestHandler<DetailsQuery, DetailsModel>
    {
        private readonly SupportManagerContext db;

        public DetailsQueryHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override async Task<DetailsModel> HandleCore(DetailsQuery request)
        {
            return await db.Users.WhereUserLoginIs(request.UserName).ProjectTo<DetailsModel>().SingleAsync();
        }
    }
}