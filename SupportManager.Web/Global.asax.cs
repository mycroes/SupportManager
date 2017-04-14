using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using SupportManager.Web.Infrastructure.Mapping;

namespace SupportManager.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");

            AutoMapperInitializer.Initialize();
        }
    }
}
