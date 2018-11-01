using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        public Service(Configuration configuration)
        {
            this.configuration = configuration;
            botClient = new TelegramBotClient(configuration.BotKey);
        }

        public bool Start(HostControl hostControl)
        {
            botClient.StartReceiving();
            bot = new SupportManagerBot(configuration.SupportManagerUri, botClient, BuildCommandHandlers(),
                Console.WriteLine);
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
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