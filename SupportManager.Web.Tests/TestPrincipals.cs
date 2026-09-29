using System.Globalization;
using System.Security.Claims;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Tests;

internal static class TestPrincipals
{
    private static readonly string True = true.ToString(CultureInfo.InvariantCulture);

    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    public static ClaimsPrincipal User(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test"));

    public static ClaimsPrincipal SuperUser() => User(new Claim(SupportManagerClaimTypes.SuperUser, True));

    public static ClaimsPrincipal Observer() => User(new Claim(SupportManagerClaimTypes.Observer, True));

    public static ClaimsPrincipal Member(int teamId) => User(MemberClaim(teamId));

    public static ClaimsPrincipal Admin(int teamId) => User(MemberClaim(teamId),
        new Claim(SupportManagerClaimTypes.TeamAdmin, teamId.ToString(CultureInfo.InvariantCulture)));

    public static Claim MemberClaim(int teamId) =>
        new(SupportManagerClaimTypes.TeamMember, teamId.ToString(CultureInfo.InvariantCulture));
}
