using Microsoft.AspNetCore.Mvc.Filters;

namespace SupportManager.Web.Infrastructure;

public class TeamMemberFilterFactory : IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<TeamMemberFilter>();

    public bool IsReusable => false;
}