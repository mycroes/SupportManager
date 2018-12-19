using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SupportManager.Telegram.DAL;
using SupportManager.Telegram.Handlers;
using SupportManager.Telegram.Infrastructure;
using Telegram.Bot;
using Topshelf;

namespace SupportManager.Telegram
{
    public class Service : ServiceControl
    {
        private readonly Configuration configuration;
        private readonly TelegramBotClient botClient;
        private SupportManagerBot bot;
        private IWebHost webHost;

        public Service(Configuration configuration)
        {
            this.configuration = configuration;
            botClient = new TelegramBotClient(configuration.BotKey);
        }

        public bool Start(HostControl hostControl)
        {
            botClient.StartReceiving();
            bot = new SupportManagerBot(configuration, botClient, BuildCommandHandlers(),
                Console.WriteLine);

            webHost = new WebHostBuilder().UseKestrel(opts => opts.ListenAnyIP(configuration.HostUri.Port))
                .ConfigureServices(services =>
                {
                    services.Add(ServiceDescriptor.Singleton(typeof(TelegramBotClient), botClient));
                    services.Add(ServiceDescriptor.Singleton(typeof(Configuration), configuration));
                }).UseStartup<Startup>().Build();

            webHost.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            webHost.Dispose();
            bot.Dispose();
            botClient.StopReceiving();
            return false;
        }

        private static Dictionary<string, SupportManagerBot.CommandHandler> BuildCommandHandlers()
        {
            return GetHandlerMethods().ToDictionary(x => x.verb, x => BuildHandler(x.method));
        }

        private static SupportManagerBot.CommandHandler BuildHandler(MethodInfo method)
        {
            var parameters = method.GetParameters();

            var resolvers = parameters.Select(p =>
            {
                if (p.ParameterType == typeof(User))
                    return Resolve(context => context.GetUser());
                if (p.ParameterType == typeof(UserDbContext))
                    return Resolve(context => Task.FromResult(context.Db));
                if (p.ParameterType == typeof(Configuration))
                    return Resolve(context => Task.FromResult(context.Configuration));
                if (p.ParameterType == typeof(ISupportManagerApi))
                    return Resolve(context => context.GetApi());
                if (p.ParameterType == typeof(Interaction))
                    return Resolve(context => Task.FromResult(context.Interaction));

                throw new Exception($"Couldn't evaluate parameter '{p.Name}'.");
            }).ToList();

            return async context =>
            {
                var handler = Activator.CreateInstance(method.DeclaringType);
                var methodArgs = new object[resolvers.Count];
                for (var i = 0; i < resolvers.Count; i++)
                    methodArgs[i] = await resolvers[i](context);

                await (Task) method.Invoke(handler, methodArgs);
            };
        }

        private static Func<CallbackQueryContext, Task<object>>
            Resolve<T>(Func<CallbackQueryContext, Task<T>> resolver) =>
            async context => await resolver(context);

        private static IEnumerable<(string verb, MethodInfo method)> GetHandlerMethods()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types.SelectMany(t =>
                t.GetMethods().Select(m => (verb: m.GetCustomAttribute<CommandAttribute>()?.Verb, Method: m))
                    .Where(x => x.verb != null));
        }
    }
}