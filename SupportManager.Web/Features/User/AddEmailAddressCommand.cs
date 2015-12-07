using System;
using MediatR;

namespace SupportManager.Web.Features.User
{
    public class AddEmailAddressCommand : IRequest
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public Func<VerifyEmailAddressCommand, string> VerificationUrlBuilder { get; set; }
    }
}