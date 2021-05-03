using System.Diagnostics;
using System.IO;
using Hangfire;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using SupportManager.Contracts;
using Topshelf;

namespace SupportManager.Web
{
    public class Program
    {
        private static string[] args;

        public static void Main()
        {
            HostFactory.Run(cfg =>
            {
                cfg.AddCommandLineDefinition("aspnetcoreargs", v => args = v.Split(' '));

                cfg.SetServiceName("SupportManager.Web");
                cfg.SetDisplayName("SupportManager.Web");
                cfg.SetDescription("SupportManager Web Interface");

                cfg.Service<IWebHost>(svc =>
                {
                    svc.ConstructUsing(CreateWebHost);
                    svc.WhenStarted(webHost =>
                    {
                        webHost.Start();
                        RecurringJob.AddOrUpdate<IForwarder>(f => f.ReadAllTeamStatus(null), Cron.MinuteInterval(10));
                    });
                    svc.WhenStopped(webHost => webHost.Dispose());
                });

                cfg.RunAsLocalSystem();
                cfg.StartAutomatically();
            });
        }

        private static IWebHost CreateWebHost()
        {
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

            if (!Debugger.IsAttached)
            {
                builder.UseHttpSys(options =>
                {
                    options.Authentication.Schemes = AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                    options.Authentication.AllowAnonymous = true;
                });

                builder.UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            }

            return builder.Build();
        }
    }
}
