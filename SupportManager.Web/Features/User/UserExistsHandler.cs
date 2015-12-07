using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class UserExistsHandler : IRequestHandler<UserExistsQuery, UserExistsResponse>
    {
        private readonly SupportManagerContext db;

        public UserExistsHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public UserExistsResponse Handle(UserExistsQuery message)
        {
            return db.Users.Where(user => user.Name == message.UserName)
                .ProjectTo<UserExistsResponse>().SingleOrDefault()
                   ?? Mapper.Map<UserExistsResponse>(message);
        }
    }
}