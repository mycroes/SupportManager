using MediatR;

namespace SupportManager.Web.Infrastructure.MediatR
{
    public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
    {
        protected abstract void Handle(TRequest request);

        public Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            Handle(request);
            return Task.CompletedTask;
        }
    }
}
