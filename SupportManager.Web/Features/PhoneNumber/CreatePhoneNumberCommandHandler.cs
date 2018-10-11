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
        private readonly IMapper mapper;

        public CreatePhoneNumberCommandHandler(SupportManagerContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        protected override async Task Handle(PhoneNumberCreateCommand message, CancellationToken cancellationToken)
        {
            var phoneNumberEntity = mapper.Map<UserPhoneNumber>(message);
            //db.PhoneNumbers.Add(phoneNumberEntity);
            await db.SaveChangesAsync();
        }
    }
}