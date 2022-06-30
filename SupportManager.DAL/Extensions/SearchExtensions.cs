using System;
using System.Linq;
using System.Linq.Expressions;
using NeinLinq;
using ReflEx;

namespace SupportManager.DAL.Extensions
{
    public static class SearchExtensions
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> queryable, string searchTerm, params Expression<Func<T, string>>[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return queryable;

            Expression<Func<string, bool>> match = s => s.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;

            return queryable.Where(searchProperties.Aggregate(default(Expression<Func<T, bool>>),
                (aggr, prop) => aggr == null ? match.Translate(prop) : aggr.Or(match.Translate(prop))));
        }
    }
}
