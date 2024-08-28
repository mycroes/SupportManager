using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;

namespace SupportManager.Web.Infrastructure.Tags;

public class EnumSelectElementBuilder : ElementTagBuilder
{
    public override bool Matches(ElementRequest subject)
    {
        var actualType = Nullable.GetUnderlyingType(subject.Accessor.PropertyType) ?? subject.Accessor.PropertyType;

        return actualType.IsEnum;
    }

    public override HtmlTag Build(ElementRequest request)
    {
        var enumType = Nullable.GetUnderlyingType(request.Accessor.PropertyType) ?? request.Accessor.PropertyType;

        var selectTag = new SelectTag(t =>
        {
            t.Option(string.Empty, string.Empty);
            foreach (var v in Enum.GetValues(enumType))
            {
                t.Option(v.ToString(), v);
            }
        });

        var value = request.RawValue;
        if (value != null) selectTag.SelectByValue(value);

        return selectTag;
    }
}