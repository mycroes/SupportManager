using MediatR;

namespace SupportManager.Web.Infrastructure.CRUD;

public record Create<T>(T Model) : IRequest;