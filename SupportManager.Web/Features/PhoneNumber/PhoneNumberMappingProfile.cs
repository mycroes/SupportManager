using AutoMapper;
using SupportManager.DAL;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class PhoneNumberMappingProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<PhoneNumberCreateCommand, UserPhoneNumber>(MemberList.Source);
            CreateMap<UserPhoneNumber, PhoneNumberListItem>();
        }
    }
}