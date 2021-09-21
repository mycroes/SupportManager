using System;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Refit;
using SupportManager.Telegram;
using SupportManager.Telegram.Infrastructure;
using Topshelf;

if (args.Length > 0)
{
    switch (args[0])
    {
        case "migrate":
            if (args.Length == 2)
            {
                var db = DbContextFactory.Create(args[1]);
                db.Database.Migrate();
            }
            else
            {
                Console.WriteLine("Usage: SupportManager.Telegram migrate <UserDb>");
            }

            return;

        case "sync":
            if (args.Length == 4)
            {
                if (!Uri.TryCreate(args[2], UriKind.Absolute, out var url))
                {
                    Console.WriteLine("Couldn't parse SupportManagerUrl argument");
                    return;
                }

                if (!Uri.TryCreate(args[3], UriKind.Absolute, out var callbackUri))
                {
                    Console.WriteLine("Couldn't parse CallbackUrl argument");
                    return;
                }

                var db = DbContextFactory.Create(args[1]);
                var callbackUrl = callbackUri.AbsoluteUri.TrimEnd('/');

                using var host = WebHost.Start(callbackUrl,
                    router => router.MapGet("/{userId}/ping", context => context.Response.WriteAsync("OK")));

                foreach (var user in await db.Users.ToListAsync())
                {
                    Console.WriteLine($"Checking user {user.UserId}");

                    try
                    {
                        var httpClient =
                            new HttpClient(new AuthenticatedHttpClientHandler(user.ApiKey)) {BaseAddress = url};
                        var api = RestService.For<ISupportManagerApi>(httpClient);

                        Console.WriteLine($"Subscribing to {callbackUrl}/{user.UserId}");
                        await api.Subscribe($"{callbackUrl}/{user.UserId}");

                        var newId = (await api.MyDetails()).Id;
                        if (newId != user.SupportManagerUserId)
                        {
                            Console.WriteLine(
                                $"Updating SupportManagerUserId for {user.ApiKey} from {user.SupportManagerUserId} to {newId}");
                            user.SupportManagerUserId = newId;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Failed to verify user {user.UserId}");
                    }
                };

                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Usage: SupportManager.Telegram sync <UserDb> <SupportManagerUrl> <HostUrl> <CallbackUrl>");
            }

            return;
    }
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