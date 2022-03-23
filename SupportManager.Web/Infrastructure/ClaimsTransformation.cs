using System.Data.Entity;
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
        if (user.IsSuperUser && !principal.HasClaim(c => c.Type == SupportManagerClaimTypes.SuperUser))
        {
            identity.AddClaim(new Claim(SupportManagerClaimTypes.SuperUser, true.ToString()));
        }

        foreach (var membership in user.Memberships)
        {
            if (!principal.HasClaim(SupportManagerClaimTypes.TeamMember, membership.TeamId.ToString()))
            {
                identity.AddClaim(new Claim(SupportManagerClaimTypes.TeamMember, membership.TeamId.ToString()));
                if (membership.IsAdministrator)
                {
                    identity.AddClaim(new Claim(SupportManagerClaimTypes.TeamAdmin, membership.TeamId.ToString()));
                }
            }
        }

        principal.AddIdentity(identity);

        return principal;
    }
}