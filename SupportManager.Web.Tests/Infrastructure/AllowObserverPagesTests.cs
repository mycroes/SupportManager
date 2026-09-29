using System.Reflection;
using SupportManager.Web.Areas.Teams.Pages;
using SupportManager.Web.Areas.Teams.Pages.Report;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Tests.Infrastructure;

public class AllowObserverPagesTests
{
    private static readonly Type[] ObservablePages =
        typeof(AllowObserverAttribute).Assembly.GetTypes()
            .Where(t => t.IsDefined(typeof(AllowObserverAttribute), false))
            .ToArray();

    [Fact]
    public void Only_Read_Only_Team_Pages_Allow_Observers()
    {
        Assert.Equal(
            new[] { typeof(FullScheduleModel), typeof(IndexModel), typeof(MonthlyModel) }.OrderBy(t => t.FullName),
            ObservablePages.OrderBy(t => t.FullName));
    }

    [Fact]
    public void Observable_Pages_Only_Have_Get_Handlers()
    {
        var handlers = ObservablePages
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            .Where(m => m.Name.StartsWith("On") && !m.Name.StartsWith("OnPage"))
            .Where(m => !m.Name.StartsWith("OnGet"))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}");

        Assert.Empty(handlers);
    }
}
