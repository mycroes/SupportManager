using System.Data.Entity;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Mailers;

namespace SupportManager.Web.Features.User
{
    public class AddEmailAddressCommandHandler : AsyncRequestHandler<AddEmailAddressCommand>
    {
        private readonly SupportManagerContext db;
        private readonly UserMailer mailer;

        public AddEmailAddressCommandHandler(SupportManagerContext db, UserMailer mailer)
        {
            this.db = db;
            this.mailer = mailer;
        }

        protected override async Task HandleCore(AddEmailAddressCommand message)
        {
            var user = await db.Users.WhereUserLoginIs(message.UserName).SingleAsync();
            var emailAddress = new UserEmailAddress {Value = message.EmailAddress};
            var code = VerificationCodeManager.GenerateCode();
            emailAddress.VerificationToken = VerificationCodeManager.GetHash(emailAddress.Value + code);

            user.EmailAddresses.Add(emailAddress);
            await db.SaveChangesAsync();

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