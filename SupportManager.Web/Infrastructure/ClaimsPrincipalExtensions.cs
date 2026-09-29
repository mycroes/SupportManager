using System.Globalization;
using System.Security.Claims;

namespace SupportManager.Web.Infrastructure;

internal static class ClaimsPrincipalExtensions
{
    private static readonly string True = true.ToString(CultureInfo.InvariantCulture);

    public static bool IsSuperUser(this ClaimsPrincipal principal) =>
        principal.HasClaim(SupportManagerClaimTypes.SuperUser, True);

    public static bool IsObserver(this ClaimsPrincipal principal) =>
        principal.HasClaim(SupportManagerClaimTypes.Observer, True);

    public static bool IsTeamMember(this ClaimsPrincipal principal, int teamId) =>
        principal.IsSuperUser() ||
        principal.HasClaim(SupportManagerClaimTypes.TeamMember, teamId.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Determines whether the principal has read access to the team, either as member or as observer.
    /// </summary>
    public static bool CanViewTeam(this ClaimsPrincipal principal, int teamId) =>
        principal.IsTeamMember(teamId) || principal.IsObserver();
}
