using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupportManager.Telegram.KeyboardMarkups
{
    public class SelectListMarkup<T> : InlineKeyboardMarkup
    {
        public SelectListMarkup(string prefix, IEnumerable<T> list, Func<T, string> labelSelector, Func<T, int> idSelector) : base(Build(prefix, list, labelSelector, idSelector))
        {
        }

        private static IEnumerable<IEnumerable<InlineKeyboardButton>> Build(string prefix, IEnumerable<T> list,
            Func<T, string> labelSelector, Func<T, int> idSelector)
        {
            InlineKeyboardButton Button(T entry) =>
                new(labelSelector(entry)) { CallbackData = prefix + idSelector(entry) };

            IEnumerable<InlineKeyboardButton> Row(T entry)
            {
                yield return Button(entry);
            }

            return list.Select(Row);
        }
    }
}