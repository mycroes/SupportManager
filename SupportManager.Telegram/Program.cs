using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SupportManager.Telegram.DAL;
using Topshelf;

namespace SupportManager.Telegram
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any() && args[0].EndsWith("migrate", StringComparison.InvariantCultureIgnoreCase))
            {
                var db = new UserDbContext();
                db.Database.Migrate();
                return;
            }

            var config = new Configuration();
            var exitCode = HostFactory.Run(cfg =>
            {
                cfg.AddCommandLineDefinition("botkey", v => config.BotKey = v);
                cfg.AddCommandLineDefinition("url", v => config.SupportManagerUri = new Uri(v));
                cfg.AddCommandLineDefinition("hostUrl", v => config.HostUri = new Uri(v));

                cfg.Service<Service>(svc =>
                {
                    svc.ConstructUsing(() => new Service(config));
                    svc.WhenStarted((s, h) => s.Start(h));
                    svc.WhenStopped((s, h) => s.Stop(h));
                });

                cfg.SetServiceName("SupportManager.Telegram");
                cfg.SetDisplayName("SupportManager.Telegram");
                cfg.SetDescription("SupportManager Telegram bot");

                cfg.RunAsNetworkService();

                cfg.StartAutomatically();
            });
        }
    }
}
