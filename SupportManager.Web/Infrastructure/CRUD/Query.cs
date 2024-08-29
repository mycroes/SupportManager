using MediatR;

namespace SupportManager.Web.Infrastructure.CRUD;

public record Query<T>(int Id) : IRequest<T>;