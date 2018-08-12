using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class AddPhoneNumberCommandHandler : AsyncRequestHandler<AddPhoneNumberCommand>
    {
        private readonly SupportManagerContext db;

        public AddPhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override async Task Handle(AddPhoneNumberCommand message, CancellationToken cancellationToken)
        {
            var user = await db.Users.WhereUserLoginIs(message.UserName).SingleAsync();
            var phoneNumber = Mapper.Map<UserPhoneNumber>(message.PhoneNumber);
            var code = VerificationCodeManager.GenerateCode();
            phoneNumber.VerificationToken = VerificationCodeManager.GetHash(phoneNumber.Value + code);

            user.PhoneNumbers.Add(Mapper.Map<UserPhoneNumber>(message.PhoneNumber));
            await db.SaveChangesAsync();
        }
    }
}