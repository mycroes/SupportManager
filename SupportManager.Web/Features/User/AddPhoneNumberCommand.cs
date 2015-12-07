using MediatR;

namespace SupportManager.Web.Features.User
{
    public class AddPhoneNumberCommand : IRequest
    {
        public string UserName { get; set; }
        public PhoneNumberCreateModel PhoneNumber { get; set; }
    }
}