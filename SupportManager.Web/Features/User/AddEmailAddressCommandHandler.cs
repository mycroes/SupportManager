using System.Linq;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Mailers;

namespace SupportManager.Web.Features.User
{
    public class AddEmailAddressCommandHandler : IRequestHandler<AddEmailAddressCommand>
    {
        private readonly SupportManagerContext db;
        private readonly UserMailer mailer;

        public AddEmailAddressCommandHandler(SupportManagerContext db, UserMailer mailer)
        {
            this.db = db;
            this.mailer = mailer;
        }

        public void Handle(AddEmailAddressCommand message)
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