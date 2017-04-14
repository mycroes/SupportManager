using Hangfire;
using StructureMap;
using SupportManager.Control.Infrastructure;
using Topshelf;

namespace SupportManager.Control
{
    public class SupportManagerService : ServiceControl
    {
        private readonly IContainer container;
        private BackgroundJobServer jobServer;

        public SupportManagerService()
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");
            container = new Container(c => c.AddRegistry<AppRegistry>());
        }

        public bool Start(HostControl hostControl)
        {
            jobServer = new BackgroundJobServer(GetJobServerOptions());
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            jobServer.Dispose();
            return true;
        }

        private BackgroundJobServerOptions GetJobServerOptions()
        {
            return new BackgroundJobServerOptions
            {
                Activator = new NestedContainerActivator(container)
            };
        }
    }
}