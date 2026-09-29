using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Tests.Infrastructure;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void Member_Can_View_And_Edit_Own_Team_Only()
    {
        var principal = TestPrincipals.Member(1);

        Assert.True(principal.IsTeamMember(1));
        Assert.True(principal.CanViewTeam(1));
        Assert.False(principal.IsTeamMember(2));
        Assert.False(principal.CanViewTeam(2));
    }

    [Fact]
    public void Observer_Can_View_All_Teams_But_Is_No_Member()
    {
        var principal = TestPrincipals.Observer();

        Assert.True(principal.IsObserver());
        Assert.True(principal.CanViewTeam(1));
        Assert.True(principal.CanViewTeam(2));
        Assert.False(principal.IsTeamMember(1));
        Assert.False(principal.IsSuperUser());
    }

    [Fact]
    public void SuperUser_Is_Member_Of_All_Teams()
    {
        var principal = TestPrincipals.SuperUser();

        Assert.True(principal.IsSuperUser());
        Assert.True(principal.IsTeamMember(1));
        Assert.True(principal.CanViewTeam(1));
        Assert.False(principal.IsObserver());
    }

    [Fact]
    public void User_Without_Claims_Has_No_Access()
    {
        var principal = TestPrincipals.Anonymous();

        Assert.False(principal.IsTeamMember(1));
        Assert.False(principal.CanViewTeam(1));
        Assert.False(principal.IsObserver());
        Assert.False(principal.IsSuperUser());
    }
}
