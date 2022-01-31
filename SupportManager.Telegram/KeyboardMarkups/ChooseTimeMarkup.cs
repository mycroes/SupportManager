using System;
using System.Collections.Generic;
using SupportManager.Telegram.Infrastructure;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupportManager.Telegram
{
    public class ChooseTimeMarkup : InlineKeyboardMarkup
    {
        public ChooseTimeMarkup(string prefix, int startHour, TimeSpan? minTime) : base(Build(prefix, startHour, minTime))
        {
        }

        private static IEnumerable<IEnumerable<InlineKeyboardButton>> Build(string prefix, int startHour, TimeSpan? minTime)
        {
            InlineKeyboardButton Button(TimeSpan time)
            {
                if (minTime > time) return new InlineKeyboardButton(" ") { CallbackData = Constants.Ignore };

                return new InlineKeyboardButton(time.ToString(@"hh\:mm")) { CallbackData = prefix + time.TotalSeconds };
            }

            IEnumerable<InlineKeyboardButton> BuildRow(int hour)
            {
                var @base = TimeSpan.FromHours(hour);
                for (var i = 0; i < 4; i++)
                    yield return Button(@base + TimeSpan.FromMinutes(15*i));
            }

            IEnumerable<InlineKeyboardButton> BuildPrevNextRow()
            {
                yield return new InlineKeyboardButton("6 hours earlier")
                {
                    CallbackData = prefix + (char)('A' + (startHour + 18) % 24)
                };
                yield return new InlineKeyboardButton("6 hours later")
                {
                    CallbackData = prefix + (char)('A' + (startHour + 6) % 24)
                };
            }

            yield return new[]
            {
                new InlineKeyboardButton($"{startHour:00}:00 - {(startHour + 6) % 24:00}:00")
                {
                    CallbackData = Constants.Ignore
                }
            };

            for (var i = 0; i < 6; i++)
                yield return BuildRow((startHour + i) % 24);

            yield return BuildPrevNextRow();
        }
    }
}