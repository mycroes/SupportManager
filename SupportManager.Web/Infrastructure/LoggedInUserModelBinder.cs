using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Infrastructure
{
    public class LoggedInUserModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var userName = bindingContext.HttpContext.User?.Identity.Name;
            if (userName != null)
            {
                var db = bindingContext.HttpContext.RequestServices.GetService<SupportManagerContext>();
                var user = await db.Users.WhereUserLoginIs(userName).SingleOrDefaultAsync();
                if (user != null) bindingContext.Result = ModelBindingResult.Success(user);
            }
        }
    }
}