using Topshelf;

namespace SupportManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(cfg =>
            {
                cfg.Service<Service>(() => new Service(args));

                cfg.AddCommandLineDefinition("aspnetcoreargs", v => args = v.Split(' '));

                cfg.SetServiceName("SupportManager.Web");
                cfg.SetDisplayName("SupportManager.Web");
                cfg.SetDescription("SupportManager Web Interface");

                cfg.RunAsNetworkService();

                cfg.StartAutomatically();
            });
        }
    }
}
