using Microsoft.AspNetCore.Mvc.ModelBinding;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure
{
    public class LoggedInUserModelBinderProvider : IModelBinderProvider {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return typeof(User) == context.Metadata.ModelType &&
                typeof(IBindLoggedInUser).IsAssignableFrom(context.Metadata.ContainerType)
                    ? new LoggedInUserModelBinder()
                    : null;
        }
    }
}