using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class ListPhoneNumbersQueryHandler : AsyncRequestHandler<PhoneNumberListQuery, IPagedList<PhoneNumberListItem>>
    {
        private readonly SupportManagerContext db;

        public ListPhoneNumbersQueryHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override async Task<IPagedList<PhoneNumberListItem>> HandleCore(PhoneNumberListQuery message)
        {
            return await db.UserPhoneNumbers.OrderBy(p => p.Label)
                .ProjectToPagedListAsync<PhoneNumberListItem>(message.PageNumber ?? 1, 5);
        }
    }
}