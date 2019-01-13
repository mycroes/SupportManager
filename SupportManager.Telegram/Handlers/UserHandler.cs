using System;
using System.Linq;
using System.Threading.Tasks;
using SupportManager.Telegram.DAL;
using SupportManager.Telegram.Infrastructure;

namespace SupportManager.Telegram.Handlers
{
    internal class UserHandler
    {
        [Command("subscribe")]
        public async Task Subscribe(ISupportManagerApi api, Interaction interaction, Configuration configuration, User user, UserDbContext db)
        {
            var opt = (SubscriptionLevel) await interaction.ReadOption(
                "What forwarding changes do you want to receive?",
                Enum.GetValues(typeof(SubscriptionLevel)).Cast<SubscriptionLevel>(), level =>
                {
                    switch (level)
                    {
                        case SubscriptionLevel.None: return "None";
                        case SubscriptionLevel.ToMe: return "Only when forwarding to me";
                        case SubscriptionLevel.FromOrToMe:
                            return "When forwarding to me, or previously was forwarded to me";
                        case SubscriptionLevel.All: return "Any change";
                        default:
                            throw new ArgumentException();
                    }
                }, level => (int) level);

            var uri = new Uri(configuration.HostUri, $"/{user.UserId}");
            await api.Subscribe(uri.ToString());

            user.SubscriptionLevel = opt;
            await db.SaveChangesAsync();

            await interaction.Write("Subscription level changed");
        }

        [Command("help")]
        public async Task Help(Interaction interaction, User user)
        {
            if (user == null)
                await interaction.Write("Use '/apikey [API Key]' to authenticate");
            else
                await interaction.Write(
                    "The following options are available:\n/get\\_status: get current and next scheduled entry\n" +
                    "/get\\_schedule: get entire forwarding schedule\n/schedule: add a new entry\n" +
                    "/forward: change forwarding status right away\n/delete: delete a scheduled entry\n" +
                    "/subscribe: subscribe to forwarding changes (or change subscription level)");
        }
    }
}