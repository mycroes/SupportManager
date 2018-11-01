using System;

namespace SupportManager.Telegram.Infrastructure
{
    public static class DateTimeOffsetMixins
    {
        public static string ToHumanReadable(this DateTimeOffset timestamp)
        {
            var dateTime = timestamp.LocalDateTime;
            var now = DateTime.Now;

            if (dateTime.Date.Equals(now.Date)) return dateTime.ToShortTimeString();
            if (dateTime.AddDays(-(int) dateTime.DayOfWeek).Date.Equals(now.AddDays(-(int) now.DayOfWeek).Date))
                return dateTime.ToString("dddd ") + dateTime.ToShortTimeString();

            return dateTime.ToString("g");
        }
    }
}