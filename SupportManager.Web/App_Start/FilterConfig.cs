using System.Web;
using System.Web.Mvc;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ValidatorActionFilter());
            filters.Add(new MvcTransactionFilter());
        }
    }
}
