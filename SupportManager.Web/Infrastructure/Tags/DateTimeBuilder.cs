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
            switch (request.RawValue)
            {
                case DateTimeOffset dt: return RenderTag(dt.LocalDateTime);
                case DateTime dt when dt.Kind == DateTimeKind.Utc: return RenderTag(dt.ToLocalTime());
                case DateTime dt: return RenderTag(dt);
                default: return RenderTag(null);
            }
        }

        private HtmlTag RenderTag(DateTime? dateTime)
        {
            var text = dateTime == null ? "-" : GetDateTimeText(dateTime.Value);
            return new HtmlTag("span").Text(text).Title(dateTime?.ToString("g")).Data("toggle", "tooltip");
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