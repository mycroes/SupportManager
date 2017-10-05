using MediatR;
using SupportManager.DAL;
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

        public IPagedList<PhoneNumberListItem> Handle(PhoneNumberListQuery message)
        {
            return null;
            //return db.PhoneNumbers.OrderBy(p => p.Label)
            //    .ProjectToPagedList<PhoneNumberListItem>(message.PageNumber ?? 1, 5);
        }
    }
}