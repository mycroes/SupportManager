using System.Web.Mvc;
using SupportManager.DAL;
using SupportManager.Web.App_Start;

namespace SupportManager.Web.Infrastructure
{
    public class MvcTransactionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Logger.Instance.Verbose("MvcTransactionFilter::OnActionExecuting");
            var context = StructuremapMvc.StructureMapDependencyScope.CurrentNestedContainer.GetInstance<SupportManagerContext>();
            context.BeginTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Logger.Instance.Verbose("MvcTransactionFilter::OnActionExecuted");
            var context = StructuremapMvc.StructureMapDependencyScope.CurrentNestedContainer.GetInstance<SupportManagerContext>();
            context.CloseTransaction(filterContext.Exception);
        }
    }
}