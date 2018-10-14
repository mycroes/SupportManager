using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure.ApiKey
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly SupportManagerContext db;

        public ApiKeyAuthenticationHandler(SupportManagerContext db, IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this.db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Query["apikey"].Count != 1) return AuthenticateResult.Fail("Invalid request");

            var key = Request.Query["apikey"][0];
            var user = await db.ApiKeys.Where(apiKey => apiKey.Value == key).Select(apiKey => apiKey.User)
                .SingleOrDefaultAsync();

            if (user == null) return AuthenticateResult.Fail("Invalid API Key");

            var claims = new[] {new Claim(ClaimTypes.Name, user.Login)};
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
