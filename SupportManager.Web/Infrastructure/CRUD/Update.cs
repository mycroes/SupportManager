using MediatR;

namespace SupportManager.Web.Infrastructure.CRUD;

public record Update<T>(int Id, T Model) : IRequest;