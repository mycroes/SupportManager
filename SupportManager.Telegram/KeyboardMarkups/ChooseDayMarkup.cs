using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupportManager.Telegram.KeyboardMarkups
{
    public class ChooseDayMarkup : InlineKeyboardMarkup
    {
        public ChooseDayMarkup(string prefix) : base(Build(prefix))
        {
        }

        private static IEnumerable<IEnumerable<InlineKeyboardButton>> Build(string prefix)
        {
            InlineKeyboardButton Button(string text, DateTime date)
            {
                return new InlineKeyboardButton(text) { CallbackData = prefix + date.ToBinary() };
            }

            IEnumerable<InlineKeyboardButton> TodayTomorrow()
            {
                yield return Button("Today", DateTime.UtcNow.Date);
                yield return Button("Tomorrow", DateTime.UtcNow.Date.AddDays(1));
            }

            IEnumerable<InlineKeyboardButton> FiveDaysAfterTomorrow()
            {
                return Enumerable.Range(2, 5).Select(i => DateTime.UtcNow.Date.AddDays(i)).Select(d =>
                    Button(d.ToString("ddd"), d));
            }

            yield return TodayTomorrow();
            yield return FiveDaysAfterTomorrow();
        }
    }
}