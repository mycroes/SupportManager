using AutoMapper.QueryableExtensions;
using X.PagedList;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace SupportManager.Web.Infrastructure
{
    public static class MapperExtensions
    {
        public static async Task<IPagedList<TDestination>> ProjectToPagedListAsync<TDestination>(
            this IOrderedQueryable queryable, IConfigurationProvider configurationProvider, int pageNumber,
            int pageSize) =>
            await queryable.ProjectTo<TDestination>(configurationProvider).ToPagedListAsync(pageNumber, pageSize);
    }
}