using System.Linq;

namespace SupportManager.Web.Features.User
{
    public static class DbSetExtensions
    {
        public static IQueryable<DAL.User> WhereUserLoginIs(this IQueryable<DAL.User> queryable, string login)
        {
            return queryable.Where(user => user.Login == login);
        }
    }
}