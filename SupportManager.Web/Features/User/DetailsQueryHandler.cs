using System.Linq;
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

        public DetailsModel Handle(DetailsQuery message)
        {
            return db.Users.WhereUserLoginIs(message.UserName)
                .ProjectTo<DetailsModel>().Single();
        }
    }
}