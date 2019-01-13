using System;
using System.ComponentModel.DataAnnotations;
using HtmlTags;
using HtmlTags.Conventions;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class SupportManagerHtmlConventions : HtmlConventionRegistry
    {
        public SupportManagerHtmlConventions()
        {
            Editors.Always.AddClass("form-control");

            Editors.IfPropertyIs<DateTimeOffset>().ModifyWith(m =>
                m.CurrentTag.Attr("type", "datetime-local")
                    .Value(m.Value<DateTimeOffset?>()?.ToLocalTime().DateTime.ToString("yyyy-MM-ddTHH:mm")));
            Editors.IfPropertyIs<DateTime?>().ModifyWith(m => m.CurrentTag
                .AddPattern("9{1,2}/9{1,2}/9999")
                .AddPlaceholder("MM/DD/YYYY")
                .AddClass("datepicker")
                .Value(m.Value<DateTime?>() != null ? m.Value<DateTime>().ToShortDateString() : string.Empty));
            Editors.If(er => er.Accessor.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase)).BuildBy(a => new HiddenTag().Value(a.StringValue()));
            Editors.IfPropertyIs<byte[]>().BuildBy(a => new HiddenTag().Value(Convert.ToBase64String(a.Value<byte[]>())));

            Editors.BuilderPolicy<UserPhoneNumberSelectElementBuilder>();
            Editors.BuilderPolicy<TeamSelectElementBuilder>();

            Labels.Always.AddClass("control-label");
            Labels.Always.AddClass("col-md-2");
            Labels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));
            DisplayLabels.Always.BuildBy<DefaultDisplayLabelBuilder>();
            DisplayLabels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));

            Displays.IfPropertyIs<DateTimeOffset>().BuildBy<DateTimeBuilder>();
            Displays.IfPropertyIs<DateTimeOffset>().ModifyWith(SetDateTimeText);
            Displays.IfPropertyIs<DateTimeOffset?>().BuildBy<DateTimeBuilder>();
            Displays.IfPropertyIs<DateTimeOffset?>().ModifyWith(SetDateTimeText);
            Displays.IfPropertyIs<DateTime>().BuildBy<DateTimeBuilder>();
            Displays.IfPropertyIs<DateTime>().ModifyWith(SetDateTimeText);
            Displays.IfPropertyIs<DateTime?>().BuildBy<DateTimeBuilder>();
            Displays.IfPropertyIs<DateTime?>().ModifyWith(SetDateTimeText);
            
            Displays.IfPropertyIs<decimal>().ModifyWith(m => m.CurrentTag.Text(m.Value<decimal>().ToString("C")));
            Displays.IfPropertyIs<bool>().BuildBy<BoolDisplayBuilder>();
        }

        public ElementCategoryExpression DisplayLabels
        {
            get { return new ElementCategoryExpression(Library.TagLibrary.Category("DisplayLabels").Profile(TagConstants.Default)); }
        }

        private static void SetDateTimeText(ElementRequest request)
        {
            request.CurrentTag.Text(DateTimeBuilder.GetText(DateTimeBuilder.GetLocalDateTime(request.RawValue)));
        }
    }
}