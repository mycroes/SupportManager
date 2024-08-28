using MediatR;

namespace SupportManager.Web.Infrastructure.CRUD;

public record Index<T> : IRequest<List<Record<T>>>;