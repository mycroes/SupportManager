using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Refit;
using SupportManager.Api;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Api.User
{
    public static class Subscription
    {
        public class Command : IRequest
        {
            public Command(string userName, string apiKey, string callbackUrl)
            {
                UserName = userName;
                ApiKey = apiKey;
                CallbackUrl = callbackUrl;
            }

            public string ApiKey { get; }
            public string CallbackUrl { get; }
            public string UserName { get; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var apiKey = await
                    db.Users.WhereUserLoginIs(request.UserName).SelectMany(u => u.ApiKeys)
                        .Where(a => a.Value == request.ApiKey).SingleOrDefaultAsync() ??
                    throw new Exception($"Failed to find configuration for ApiKey {request.ApiKey}.");

                var callback = RestService.For<ICallback>(request.CallbackUrl);
                try
                {
                    await callback.Ping();
                }
                catch (Exception)
                {
                    throw new Exception($"Failed to ping to callback URL {request.CallbackUrl}.");
                }

                apiKey.CallbackUrl = request.CallbackUrl;
                await db.SaveChangesAsync();
            }
        }
    }
}
