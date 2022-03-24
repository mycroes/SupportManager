using System.Data.Entity;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Infrastructure;

internal class ClaimsTransformation : IClaimsTransformation
{
    private readonly SupportManagerContext db;

    public ClaimsTransformation(SupportManagerContext db)
    {
        this.db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.Name == null) return principal;

        var user = await db.Users.WhereUserLoginIs(principal.Identity!.Name).Include(u => u.Memberships)
            .SingleOrDefaultAsync();

        if (user == null) return principal;

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(SupportManagerClaimTypes.UserId, user.Id.ToString(CultureInfo.InvariantCulture)));
        if (user.IsSuperUser && !principal.HasClaim(c => c.Type == SupportManagerClaimTypes.SuperUser))
        {
            identity.AddClaim(
                new Claim(SupportManagerClaimTypes.SuperUser, true.ToString(CultureInfo.InvariantCulture)));
        }

        foreach (var membership in user.Memberships)
        {
            var teamId = membership.TeamId.ToString(CultureInfo.InvariantCulture);
            if (principal.HasClaim(SupportManagerClaimTypes.TeamMember, teamId)) continue;

            identity.AddClaim(new Claim(SupportManagerClaimTypes.TeamMember, teamId));
            if (membership.IsAdministrator)
            {
                identity.AddClaim(new Claim(SupportManagerClaimTypes.TeamAdmin, teamId));
            }
        }

        principal.AddIdentity(identity);

        return principal;
    }
}