using Microsoft.AspNetCore.Mvc.Filters;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure
{
    public class DbContextTransactionFilter : IAsyncActionFilter, IAsyncPageFilter
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

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            try
            {
                _dbContext.BeginTransaction();

                var actionExecuted = await next();
                if (actionExecuted.Exception != null && !actionExecuted.ExceptionHandled)
                {
                    _dbContext.RollbackTransaction();
                }
                else
                {
                    await _dbContext.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }
    }
}
