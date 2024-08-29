using MediatR;

namespace SupportManager.Web.Infrastructure.CRUD;

public record Delete<T>(int Id) : IRequest;