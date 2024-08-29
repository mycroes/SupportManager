using MediatR;

namespace SupportManager.Web.Infrastructure.MediatR;

public abstract class AsyncRequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
{
    protected abstract Task Handle(TRequest request, CancellationToken cancellationToken);

    Task IRequestHandler<TRequest>.Handle(TRequest request, CancellationToken cancellationToken) =>
        Handle(request, cancellationToken);
}