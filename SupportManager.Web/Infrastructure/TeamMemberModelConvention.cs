using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SupportManager.Web.Infrastructure;

public class TeamMemberModelConvention : IPageApplicationModelConvention
{
    public void Apply(PageApplicationModel model)
    {
        if (model.AreaName != "Teams") return;

        model.Filters.Add(new TeamMemberFilterFactory());
    }
}