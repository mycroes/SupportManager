using MediatR;

namespace SupportManager.Web.Features.User
{
    public class SetPrimaryEmailAddressCommand : IRequest
    {
        public string UserName { get; set; }
        public int PrimaryEmailAddressId { get; set; }
    }
}