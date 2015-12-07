using HtmlTags.Conventions;
using StructureMap.Configuration.DSL;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class HtmlTagRegistry : Registry
    {

        public HtmlTagRegistry()
        {
            var htmlConventionLibrary = new HtmlConventionLibrary();
            new DefaultAspNetMvcHtmlConventions().Apply(htmlConventionLibrary);
            new DefaultHtmlConventions().Apply(htmlConventionLibrary);
            For<HtmlConventionLibrary>().Use(htmlConventionLibrary);
        }
    }
}