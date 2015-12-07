using MediatR;

namespace SupportManager.Web.Features.User
{
    public class SetPrimaryPhoneNumberCommand : IRequest
    {
        public string UserName { get; set; }
        public int PrimaryPhoneNumberId { get; set; }
    }
}