using MediatR;

namespace SupportManager.Web.Features.User
{
    public class UserExistsQuery : IRequest<UserExistsResponse>
    {
        public string UserName { get; set; }
    }
}