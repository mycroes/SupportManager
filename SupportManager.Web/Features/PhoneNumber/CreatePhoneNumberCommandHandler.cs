using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class CreatePhoneNumberCommandHandler : AsyncRequestHandler<PhoneNumberCreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreatePhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override async Task Handle(PhoneNumberCreateCommand message, CancellationToken cancellationToken)
        {
            var phoneNumberEntity = Mapper.Map<UserPhoneNumber>(message);
            //db.PhoneNumbers.Add(phoneNumberEntity);
            await db.SaveChangesAsync();
        }
    }
}