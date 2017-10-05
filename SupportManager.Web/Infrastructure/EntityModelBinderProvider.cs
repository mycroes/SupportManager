using Microsoft.AspNetCore.Mvc.ModelBinding;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure
{
    public class EntityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return typeof(Entity).IsAssignableFrom(context.Metadata.ModelType) ? new EntityModelBinder() : null;
        }
    }
}