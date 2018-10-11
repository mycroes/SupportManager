using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class ListPhoneNumbersQueryHandler : IRequestHandler<PhoneNumberListQuery, IPagedList<PhoneNumberListItem>>
    {
        private readonly SupportManagerContext db;
        private readonly IMapper mapper;

        public ListPhoneNumbersQueryHandler(SupportManagerContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IPagedList<PhoneNumberListItem>> Handle(PhoneNumberListQuery message,
            CancellationToken cancellationToken)
        {
            return await db.UserPhoneNumbers.OrderBy(p => p.Label)
                .ProjectToPagedListAsync<PhoneNumberListItem>(mapper.ConfigurationProvider, message.PageNumber ?? 1, 5);
        }
    }
}