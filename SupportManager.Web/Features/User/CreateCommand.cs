using MediatR;

namespace SupportManager.Web.Features.User
{
    public class CreateCommand : IRequest
    {
        public string Name { get; set; }
    }
}