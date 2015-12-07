using System.Linq;

namespace SupportManager.Web.Features.User
{
    public static class DbSetExtensions
    {
        public static IQueryable<DAL.User> WhereUserNameIs(this IQueryable<DAL.User> queryable, string userName)
        {
            return queryable.Where(user => user.Name == userName);
        }
    }
}