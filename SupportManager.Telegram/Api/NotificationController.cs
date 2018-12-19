using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportManager.Api.Events;
using SupportManager.Telegram.DAL;
using Telegram.Bot;

namespace SupportManager.Telegram.Api
{
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly UserDbContext db;
        private readonly TelegramBotClient botClient;

        public NotificationController(UserDbContext db, TelegramBotClient botClient)
        {
            this.db = db;
            this.botClient = botClient;
        }

        [Route("{userId:int}/ping")]
        public async Task<ActionResult> GetUser(int userId)
        {
            var user = await db.Users.Where(u => u.UserId == userId).SingleOrDefaultAsync() ??
                throw new Exception($"Invalid userId ({userId}) specified.");

            return Ok();
        }

        [HttpPost("{userId:int}/notify")]
        public async Task<ActionResult> Index(int userId, [FromBody] ForwardChanged change)
        {
            var user = await db.Users.Where(u => u.UserId == userId).SingleOrDefaultAsync() ??
                throw new Exception($"Invalid userId ({userId}) specified.");
            if (IsSubscribed(user.SubscriptionLevel, change, user.SupportManagerUserId))
                await botClient.SendTextMessageAsync(user.ChatId,
                    $"Forward changed from {change.OldForward.User.DisplayName} to {change.NewForward.User.DisplayName}");
            
            return Ok("Success");
        }

        private static bool IsSubscribed(SubscriptionLevel level, ForwardChanged change, int userId)
        {
            if (level == SubscriptionLevel.All) return true;
            if (change.NewForward.User.Id == userId)
                return level == SubscriptionLevel.ToMe || level == SubscriptionLevel.FromOrToMe;
            if (change.OldForward.User.Id == userId)
                return level == SubscriptionLevel.FromOrToMe;

            return false;
        }
    }
}