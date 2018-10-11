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
        private readonly IMapper mapper;

        public AddPhoneNumberCommandHandler(SupportManagerContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        protected override async Task Handle(AddPhoneNumberCommand message, CancellationToken cancellationToken)
        {
            var user = await db.Users.WhereUserLoginIs(message.UserName).SingleAsync();
            var phoneNumber = mapper.Map<UserPhoneNumber>(message.PhoneNumber);
            var code = VerificationCodeManager.GenerateCode();
            phoneNumber.VerificationToken = VerificationCodeManager.GetHash(phoneNumber.Value + code);

            user.PhoneNumbers.Add(mapper.Map<UserPhoneNumber>(message.PhoneNumber));
            await db.SaveChangesAsync();
        }
    }
}