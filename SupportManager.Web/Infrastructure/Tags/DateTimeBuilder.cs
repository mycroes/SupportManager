using System;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class DateTimeBuilder : IElementBuilder
    {
        public HtmlTag Build(ElementRequest request)
        {
            return RenderTag(GetLocalDateTime(request.RawValue));
        }

        public static DateTime? GetLocalDateTime(object value)
        {
            switch (value)
            {
                case DateTimeOffset dt: return dt.LocalDateTime;
                case DateTime dt when dt.Kind == DateTimeKind.Utc: return dt.ToLocalTime();
                case DateTime dt: return dt;
                default: return null;
            }
        }

        public static string GetText(DateTime? dateTime)
        {
            return dateTime == null ? "-" : GetDateTimeText(dateTime.Value);
        }

        private static HtmlTag RenderTag(DateTime? dateTime)
        {
            var text = GetText(dateTime);
            var tag = new HtmlTag("span").Text(text);

            return dateTime == null ? tag : tag.Title(dateTime.Value.ToString("g")).Data("toggle", "tooltip");
        }

        private static string GetDateTimeText(DateTime dateTime)
        {
            var now = DateTime.Now;
            if (dateTime.Date.Equals(now.Date)) return dateTime.ToShortTimeString();
            if (dateTime.AddDays(-(int) dateTime.DayOfWeek).Date.Equals(now.AddDays(-(int) now.DayOfWeek).Date))
                return dateTime.ToString("dddd ") + dateTime.ToShortTimeString();
            return dateTime.ToString("g");
        }
    }
}