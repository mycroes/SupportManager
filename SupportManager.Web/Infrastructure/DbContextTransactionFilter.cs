using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure
{
    public class DbContextTransactionFilter : IAsyncActionFilter
    {
        private readonly SupportManagerContext _dbContext;

        public DbContextTransactionFilter(SupportManagerContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                _dbContext.BeginTransaction();

                await next();

                await _dbContext.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }
    }
}
