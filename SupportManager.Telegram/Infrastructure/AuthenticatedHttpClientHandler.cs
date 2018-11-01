using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SupportManager.Telegram.Infrastructure
{
    internal class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private readonly string apiKey;

        public AuthenticatedHttpClientHandler(string apiKey)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));

            this.apiKey = apiKey;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Add("X-API-Key", apiKey);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}