using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DelegateDecompiler;
using X.PagedList;

namespace SupportManager.Web.Infrastructure
{
    public static class MapperExtensions
    {
        public static async Task<IPagedList<TDestination>> ProjectToPagedListAsync<TDestination>(
            this IOrderedQueryable queryable, IConfigurationProvider configurationProvider, int pageNumber,
            int pageSize) =>
            await queryable.ProjectTo<TDestination>(configurationProvider).Decompile()
                .ToPagedListAsync(pageNumber, pageSize);
    }
}