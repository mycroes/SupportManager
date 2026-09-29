using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Tests.Infrastructure;

public class TeamMemberFilterTests
{
    private const int TeamId = 1;

    [Fact]
    public void Ignores_Pages_Outside_Teams_Area()
    {
        var context = CreateContext(TestPrincipals.Anonymous(), typeof(ReadOnlyPage), area: "Admin");

        Assert.Null(Execute(context));
    }

    [Fact]
    public void Permits_SuperUser()
    {
        var context = CreateContext(TestPrincipals.SuperUser(), typeof(EditPage), method: "POST");

        Assert.Null(Execute(context));
    }

    [Fact]
    public void Rejects_Request_Without_TeamId()
    {
        var context = CreateContext(TestPrincipals.Member(TeamId), typeof(ReadOnlyPage), teamId: null);

        Assert.IsType<BadRequestResult>(Execute(context));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public void Permits_Member(string method)
    {
        var context = CreateContext(TestPrincipals.Member(TeamId), typeof(EditPage), method: method);

        Assert.Null(Execute(context));
    }

    [Fact]
    public void Hides_Team_From_Non_Member()
    {
        var context = CreateContext(TestPrincipals.Member(TeamId + 1), typeof(ReadOnlyPage));

        Assert.IsType<NotFoundResult>(Execute(context));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    public void Permits_Observer_To_Read_Observable_Page(string method)
    {
        var context = CreateContext(TestPrincipals.Observer(), typeof(ReadOnlyPage), method: method);

        Assert.Null(Execute(context));
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public void Forbids_Observer_To_Write_Observable_Page(string method)
    {
        var context = CreateContext(TestPrincipals.Observer(), typeof(ReadOnlyPage), method: method);

        Assert.IsType<ForbidResult>(Execute(context));
    }

    [Fact]
    public void Forbids_Observer_On_Page_Without_Observer_Access()
    {
        var context = CreateContext(TestPrincipals.Observer(), typeof(EditPage));

        Assert.IsType<ForbidResult>(Execute(context));
    }

    [Fact]
    public void Forbids_Observer_On_Team_Admin_Page()
    {
        var context = CreateContext(TestPrincipals.Observer(), typeof(EditPage), viewEnginePath: "/Admin/Members/Index");

        Assert.IsType<ForbidResult>(Execute(context));
    }

    [Fact]
    public void Observer_That_Is_Member_Has_Member_Access()
    {
        var principal = TestPrincipals.Observer();
        ((ClaimsIdentity) principal.Identity!).AddClaim(TestPrincipals.MemberClaim(TeamId));
        var context = CreateContext(principal, typeof(EditPage), method: "POST");

        Assert.Null(Execute(context));
    }

    [Fact]
    public void Forbids_Member_On_Team_Admin_Page()
    {
        var context = CreateContext(TestPrincipals.Member(TeamId), typeof(EditPage),
            viewEnginePath: "/Admin/Members/Index");

        Assert.IsType<ForbidResult>(Execute(context));
    }

    [Fact]
    public void Permits_Team_Admin_On_Team_Admin_Page()
    {
        var context = CreateContext(TestPrincipals.Admin(TeamId), typeof(EditPage),
            viewEnginePath: "/Admin/Members/Index");

        Assert.Null(Execute(context));
    }

    private static IActionResult? Execute(PageHandlerExecutingContext context)
    {
        new TeamMemberFilter().OnPageHandlerExecuting(context);

        return context.Result;
    }

    private static PageHandlerExecutingContext CreateContext(ClaimsPrincipal principal, Type pageType,
        string method = "GET", string area = "Teams", string viewEnginePath = "/Index", int? teamId = TeamId)
    {
        var httpContext = new DefaultHttpContext { User = principal };
        httpContext.Request.Method = method;

        var routeData = new RouteData();
        if (teamId != null) routeData.Values["teamId"] = teamId.Value.ToString();

        var actionDescriptor = new CompiledPageActionDescriptor
        {
            AreaName = area, ViewEnginePath = viewEnginePath, HandlerTypeInfo = pageType.GetTypeInfo()
        };

        var pageContext = new PageContext(new ActionContext(httpContext, routeData, actionDescriptor));

        return new PageHandlerExecutingContext(pageContext, new List<IFilterMetadata>(), null,
            new Dictionary<string, object?>(), new object());
    }

    [AllowObserver]
    private class ReadOnlyPage : PageModel
    {
    }

    private class EditPage : PageModel
    {
    }
}
