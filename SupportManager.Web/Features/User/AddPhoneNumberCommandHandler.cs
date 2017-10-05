using System.Linq;
using AutoMapper;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class AddPhoneNumberCommandHandler : IRequestHandler<AddPhoneNumberCommand>
    {
        private readonly SupportManagerContext db;

        public AddPhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public void Handle(AddPhoneNumberCommand message)
        {
            var user = db.Users.WhereUserLoginIs(message.UserName).Single();
            var phoneNumber = Mapper.Map<UserPhoneNumber>(message.PhoneNumber);
            var code = VerificationCodeManager.GenerateCode();
            phoneNumber.VerificationToken = VerificationCodeManager.GetHash(phoneNumber.Value + code);

            user.PhoneNumbers.Add(Mapper.Map<UserPhoneNumber>(message.PhoneNumber));
            db.SaveChanges();
        }
    }
}