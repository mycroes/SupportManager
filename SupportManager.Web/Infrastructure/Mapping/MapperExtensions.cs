using System.Linq;
using AutoMapper.QueryableExtensions;
using DelegateDecompiler;
using PagedList;

namespace SupportManager.Web.Infrastructure.Mapping
{
    public static class MapperExtensions
    {
        public static IPagedList<TDestination> ProjectToPagedList<TDestination>(this IOrderedQueryable queryable,
            int pageNumber, int pageSize)
        {
            return queryable.ProjectTo<TDestination>().Decompile().ToPagedList(pageNumber, pageSize);
        }
    }
}