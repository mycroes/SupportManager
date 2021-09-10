using System;
using Microsoft.EntityFrameworkCore;
using SupportManager.Telegram;
using SupportManager.Telegram.Infrastructure;
using Topshelf;


if (args.Length == 2 && args[0].Equals("migrate", StringComparison.InvariantCultureIgnoreCase))
{
    var db = DbContextFactory.Create(args[1]);
    db.Database.Migrate();

    return;
}

HostFactory.Run(cfg =>
{
    var config = new Configuration();

    cfg.AddCommandLineDefinition("db", v => config.DbFileName = v);
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