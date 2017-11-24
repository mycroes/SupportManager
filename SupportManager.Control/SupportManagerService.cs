using Hangfire;
using Hangfire.Console;
using StructureMap;
using SupportManager.Contracts;
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
            GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire").UseConsole();
            container = new Container(c => c.AddRegistry<AppRegistry>());
        }

        public bool Start(HostControl hostControl)
        {
            jobServer = new BackgroundJobServer(GetJobServerOptions());
            RecurringJob.AddOrUpdate<IForwarder>(f => f.ReadAllTeamStatus(null), Cron.Minutely);
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