using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SupportManager.Web.Infrastructure
{
    public class TeamIdPageRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            if (model.AreaName != "Teams") return;

            var selectorCount = model.Selectors.Count;
            for (var i = 0; i < selectorCount; i++)
            {
                var selector = model.Selectors[i];
                if (selector.AttributeRouteModel?.Template?.StartsWith("Teams") ?? false)
                {
                    selector.AttributeRouteModel.Template = "Teams/{teamId:int}" +
                        selector.AttributeRouteModel.Template.Substring("Teams".Length);
                }
            }
        }
    }
}