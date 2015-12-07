using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class BoolDisplayBuilder : IElementBuilder
    {
        public HtmlTag Build(ElementRequest request)
        {
            var @class = (bool) request.RawValue ? "ok" : "remove";
            return new HtmlTag("i").AddClasses($"glyphicon glyphicon-{@class}");
        }
    }
}