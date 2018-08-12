using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class VerifyEmailAddressCommand : IRequest<EmailAddressVerificationResult>
    {
        public int Id { get; set; }
        public string VerificationCode { get; set; }
    }

    public class EmailAddressVerificationResult
    {
        public bool Success { get; set; }
        public int EmailAddressId { get; set; }
    }

    public class VerifyEmailAddressCommandHandler : IRequestHandler<VerifyEmailAddressCommand, EmailAddressVerificationResult>
    {
        private readonly SupportManagerContext db;

        public VerifyEmailAddressCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public async Task<EmailAddressVerificationResult> Handle(VerifyEmailAddressCommand message,
            CancellationToken cancellationToken)
        {
            var emailAddress = await db.UserEmailAddresses.FindAsync(message.Id);
            var hash =
                emailAddress.VerificationToken =
                    VerificationCodeManager.GetHash(emailAddress.Value + message.VerificationCode);

            if (emailAddress.VerificationToken != hash)
            {
                return new EmailAddressVerificationResult
                {
                    Success = false,
                    EmailAddressId = emailAddress.Id
                };
            }

            emailAddress.VerificationToken = null;
            emailAddress.IsVerified = true;

            return new EmailAddressVerificationResult {Success = true};
        }
    }
}