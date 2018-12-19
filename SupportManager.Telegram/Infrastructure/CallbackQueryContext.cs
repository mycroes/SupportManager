using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Refit;
using SupportManager.Telegram.DAL;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = SupportManager.Telegram.DAL.User;

namespace SupportManager.Telegram.Infrastructure
{
    internal class CallbackQueryContext : IDisposable
    {
        public Configuration Configuration { get; }

        private readonly ITelegramBotClient bot;
        private readonly Message botMessage;
        private readonly string input;

        private readonly Lazy<UserDbContext> dbContext = new Lazy<UserDbContext>();
        private readonly Lazy<Interaction> inputReader;

        private ISupportManagerApi api;
        private User user;

        public CallbackQueryContext(Configuration configuration, ITelegramBotClient bot, Message botMessage, string input)
        {
            Configuration = configuration;
            this.bot = bot;
            this.botMessage = botMessage;
            this.input = input;

            inputReader = new Lazy<Interaction>(BuildInputReader);
        }

        public UserDbContext Db => dbContext.Value;
        public Interaction Interaction => inputReader.Value;
        public string Command => Interaction.Command;

        private Interaction BuildInputReader()
        {
            return new Interaction(bot, input, botMessage, this);
        }

        public async Task<ISupportManagerApi> GetApi(string apiKey = null)
        {
            if (api != null) return api;

            if (apiKey == null)
            {
                await RequireUser();
                apiKey = user.ApiKey;
            }

            var httpClient =
                new HttpClient(new AuthenticatedHttpClientHandler(apiKey)) {BaseAddress = Configuration.SupportManagerUri};
            api = RestService.For<ISupportManagerApi>(httpClient);

            return api;
        }

        public async Task<User> GetUser()
        {
            if (user == null)
                user = await Db.Users.Where(u => u.ChatId == botMessage.Chat.Id).SingleOrDefaultAsync();

            return user;
        }

        private async Task RequireUser()
        {
            if (await GetUser() == null) throw new AuthenticationRequiredException();
        }

        public void Dispose()
        {
            if (dbContext.IsValueCreated) dbContext.Value.Dispose();
        }
    }
}
