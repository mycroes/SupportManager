using AutoMapper;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure.CommandProcessing;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class CreatePhoneNumberCommandHandler : RequestHandler<PhoneNumberCreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreatePhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override void HandleCore(PhoneNumberCreateCommand message)
        {
            var phoneNumberEntity = Mapper.Map<UserPhoneNumber>(message);
            //db.PhoneNumbers.Add(phoneNumberEntity);
        }
    }
}