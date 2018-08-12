using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class ListPhoneNumbersQueryHandler : IRequestHandler<PhoneNumberListQuery, IPagedList<PhoneNumberListItem>>
    {
        private readonly SupportManagerContext db;

        public ListPhoneNumbersQueryHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public async Task<IPagedList<PhoneNumberListItem>> Handle(PhoneNumberListQuery message,
            CancellationToken cancellationToken)
        {
            return await db.UserPhoneNumbers.OrderBy(p => p.Label)
                .ProjectToPagedListAsync<PhoneNumberListItem>(message.PageNumber ?? 1, 5);
        }
    }
}