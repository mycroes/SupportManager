using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using DelegateDecompiler;
using X.PagedList;

namespace SupportManager.Web.Infrastructure
{
    public static class MapperExtensions
    {
        public static IPagedList<TDestination> ProjectToPagedList<TDestination>(this IOrderedQueryable queryable,
            int pageNumber, int pageSize)
        {
            return queryable.ProjectTo<TDestination>().Decompile().ToPagedList(pageNumber, pageSize);
        }

        public static async Task<IPagedList<TDestination>> ProjectToPagedListAsync<TDestination>(
            this IOrderedQueryable queryable, int pageNumber, int pageSize)
        {
            return await queryable.ProjectTo<TDestination>().Decompile().ToPagedListAsync(pageNumber, pageSize);
        }
    }
}