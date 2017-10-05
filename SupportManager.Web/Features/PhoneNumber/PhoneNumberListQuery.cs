using MediatR;
using X.PagedList;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class PhoneNumberListQuery : IRequest<IPagedList<PhoneNumberListItem>>
    {
        public int? PageNumber { get; set; }
    }
}