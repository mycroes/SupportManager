using AutoMapper;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class CreatePhoneNumberCommandHandler : IRequestHandler<PhoneNumberCreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreatePhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public void Handle(PhoneNumberCreateCommand message)
        {
            var phoneNumberEntity = Mapper.Map<UserPhoneNumber>(message);
            //db.PhoneNumbers.Add(phoneNumberEntity);
        }
    }
}