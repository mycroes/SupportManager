using System.Globalization;
using System.Security.Claims;

namespace SupportManager.Web.Infrastructure;

internal static class ClaimsPrincipalExtensions
{
    public static bool IsTeamMember(this ClaimsPrincipal principal, int teamId) =>
        principal.HasClaim(SupportManagerClaimTypes.SuperUser, true.ToString(CultureInfo.InvariantCulture)) ||
        principal.HasClaim(SupportManagerClaimTypes.TeamMember, teamId.ToString(CultureInfo.InvariantCulture));
}
