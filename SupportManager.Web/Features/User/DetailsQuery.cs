using MediatR;

namespace SupportManager.Web.Features.User
{
    public class DetailsQuery : IRequest<DetailsModel>
    {
        public string UserName { get; set; }
    }
}