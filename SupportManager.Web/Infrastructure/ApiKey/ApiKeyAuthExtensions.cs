using System;
using Microsoft.AspNetCore.Authentication;

namespace SupportManager.Web.Infrastructure.ApiKey
{
    public static class ApiKeyAuthExtensions
    {
        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder) =>
            builder.AddApiKeyAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder,
            string authenticationScheme, Action<ApiKeyAuthenticationOptions> configureOptions) =>
            builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme,
                configureOptions);
    }
}