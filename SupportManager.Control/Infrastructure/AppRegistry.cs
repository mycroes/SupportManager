using System.Net.Mail;
using System.Threading.Tasks;
using StructureMap;
using StructureMap.Pipeline;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Control.Infrastructure
{
    public class AppRegistry : Registry
    {
        public AppRegistry()
        {
            For<IForwarder>().Use<Forwarder>();
            For<IPublisher>().Use<Publisher>();

            Scan(
                scan => {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                    scan.LookForRegistries();
                });
            For<SupportManagerContext>()
                .Use(() => new SupportManagerContext("SupportManagerContext"))
                .LifecycleIs<TransientLifecycle>();
            For<SmtpClient>()
                .Use(() => new SmtpClient())
                .LifecycleIs<TransientLifecycle>();

            var scheduler = new ConcurrentExclusiveSchedulerPair();
            For<TaskFactory>().Use(new TaskFactory(scheduler.ExclusiveScheduler)).Singleton();
        }
    }
}