using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class UserExistsHandler : AsyncRequestHandler<UserExistsQuery, UserExistsResponse>
    {
        private readonly SupportManagerContext db;

        public UserExistsHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override async Task<UserExistsResponse> HandleCore(UserExistsQuery request)
        {
            return await db.Users.Where(user => user.Login == request.UserName).ProjectTo<UserExistsResponse>()
                .SingleOrDefaultAsync() ?? Mapper.Map<UserExistsResponse>(request);
        }
    }
}