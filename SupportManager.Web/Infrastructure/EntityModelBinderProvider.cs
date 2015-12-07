using System;
using System.Web.Mvc;
using SupportManager.DAL;
using SupportManager.Web.App_Start;

namespace SupportManager.Web.Infrastructure
{
    public class EntityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            return typeof (Entity).IsAssignableFrom(modelType)
                ? StructuremapMvc.StructureMapDependencyScope.GetInstance<EntityModelBinder>()
                : null;
        }
    }

    public class EntityModelBinder : IModelBinder
    {
        private readonly SupportManagerContext dbContext;
        public EntityModelBinder(SupportManagerContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var attemptedValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;

            return string.IsNullOrWhiteSpace(attemptedValue) ? null
                : dbContext.Set(bindingContext.ModelType).Find(int.Parse(attemptedValue));
        }
    }
}