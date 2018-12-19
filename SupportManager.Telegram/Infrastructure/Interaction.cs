using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupportManager.Api.Teams;
using SupportManager.Telegram.KeyboardMarkups;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupportManager.Telegram.Infrastructure
{
    internal class Interaction
    {
        private readonly ITelegramBotClient bot;
        private readonly string input;
        private readonly Message message;
        private readonly CallbackQueryContext context;
        private readonly List<string> segments;
        private int index = 1; // Skip command

        public Interaction(ITelegramBotClient bot, string input, Message message, CallbackQueryContext context)
        {
            this.bot = bot;
            this.input = input;
            this.message = message;
            this.context = context;

            segments = input.Split(';').ToList();
        }

        public string Command => segments[0];

        public async Task<int> ReadTeam(string text)
        {
            var api = await context.GetApi();
            var teams = await api.MyTeams();
            if (index < segments.Count && int.TryParse(segments[index], out var teamId))
            {
                index++;
                return teamId;
            }

            if (teams.Count == 1)
            {
                segments.Insert(index, teams[0].Id.ToString());
                index++;
                return teams[0].Id;
            }

            throw await QueryInput(text,
                new SelectListMarkup<Team>(BuildQuery(), teams, team => team.Name, team => team.Id));
        }

        public async Task<int> ReadPhoneNumber(string text, int teamId)
        {
            if (index < segments.Count && int.TryParse(segments[index], out var phoneNumberId))
            {
                index++;
                return phoneNumberId;
            }

            var api = await context.GetApi();
            var users = await api.GetMembers(teamId);

            throw await QueryInput(text,
                Select(BuildQuery(), users.SelectMany(u => u.PhoneNumbers.Select(p => (user: u, phoneNumber: p))),
                    x => $"{x.user.DisplayName} - {x.phoneNumber.Value}", x => x.phoneNumber.Id));
        }

        public async Task<int> ReadOption<T>(string text, IEnumerable<T> options, Func<T, string> labelSelector,
            Func<T, int> idSelector)
        {
            if (index < segments.Count && int.TryParse(segments[index], out var id))
            {
                index++;
                return id;
            }

            throw await QueryInput(text, Select(BuildQuery(), options, labelSelector, idSelector));
        }

        public async Task<TimeSpan> ReadTime(string text, TimeSpan? minTime = null)
        {
            if (index < segments.Count && segments[index].Length > 0)
            {
                var first = segments[index][0];
                if (first >= 'A' && first < 'A' + 24)
                    throw await QueryInput(text, new ChooseTimeMarkup(BuildQuery(), first - 'A', minTime));

                if (int.TryParse(segments[index], out var seconds))
                {
                    var parsed = TimeSpan.FromSeconds(seconds);
                    if (minTime == null || parsed > minTime)
                    {
                        index++;
                        return parsed;
                    }
                }
            }

            throw await QueryInput(text, new ChooseTimeMarkup(BuildQuery(), minTime?.Hours ?? 8, minTime));
        }

        public async Task<DateTime> ReadDate(string text)
        {
            if (index < segments.Count && long.TryParse(segments[index], out var binDate))
            {
                index++;
                return DateTime.FromBinary(binDate);
            }

            throw await QueryInput(text, new ChooseDayMarkup(BuildQuery()));
        }

        public async Task Write(string text)
        {
            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, ParseMode.Markdown);
        }

        private static SelectListMarkup<T> Select<T>(string prefix, IEnumerable<T> input, Func<T, string> labelSelector,
            Func<T, int> idSelector) =>
            new SelectListMarkup<T>(prefix, input, labelSelector, idSelector);

        private string BuildQuery(string data = null)
        {
            var path = string.Join(";", segments.Take(index));
            if (!string.IsNullOrWhiteSpace(data)) return path + ";" + data;

            return path + ";";
        }

        private async Task<IncompleteDataException> QueryInput(string text, InlineKeyboardMarkup replyMarkup)
        {
            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, text, ParseMode.Markdown,
                replyMarkup: replyMarkup);

            return new IncompleteDataException();
        }
    }
}
