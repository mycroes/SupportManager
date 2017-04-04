using System.Diagnostics;
using Topshelf;

namespace SupportManager.Control
{
    class Program
    {
        static void Main()
        {
            var exitCode = HostFactory.Run(config =>
            {
                config.Service<SupportManagerService>();

                config.RunAsLocalSystem();

                config.SetDescription("Performs scheduled forwarding and reads forwarding state at intervals");
                config.SetDisplayName("SupportManager Control");
                config.SetServiceName("SupportManager.Control");

                config.StartAutomatically();
            });

            if (exitCode != TopshelfExitCode.Ok && Debugger.IsAttached) Debugger.Break();
        }
    }
}
