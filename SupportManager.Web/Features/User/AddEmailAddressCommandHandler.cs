using System.Linq;
using AutoMapper;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure.CommandProcessing;
using SupportManager.Web.Mailers;

namespace SupportManager.Web.Features.User
{
    public class AddEmailAddressCommandHandler : RequestHandler<AddEmailAddressCommand>
    {
        private readonly SupportManagerContext db;
        private readonly UserMailer mailer;

        public AddEmailAddressCommandHandler(SupportManagerContext db, UserMailer mailer)
        {
            this.db = db;
            this.mailer = mailer;
        }

        protected override void HandleCore(AddEmailAddressCommand message)
        {
            var user = db.Users.WhereUserLoginIs(message.UserName).Single();
            var emailAddress = new UserEmailAddress {Value = message.EmailAddress};
            var code = VerificationCodeManager.GenerateCode();
            emailAddress.VerificationToken = VerificationCodeManager.GetHash(emailAddress.Value + code);

            user.EmailAddresses.Add(emailAddress);
            db.SaveChanges();

            var urlCommand = new VerifyEmailAddressCommand
            {
                Id = emailAddress.Id,
                VerificationCode = code
            };

            var url = message.VerificationUrlBuilder(urlCommand);
            mailer.EmailVerificationCode(emailAddress.Value, user.Login, url);
        }
    }
}