using AutoMapper;
using SupportManager.DAL;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class PhoneNumberMappingProfile : Profile
    {
        public PhoneNumberMappingProfile()
        {
            CreateMap<PhoneNumberCreateCommand, UserPhoneNumber>(MemberList.Source);
            CreateMap<UserPhoneNumber, PhoneNumberListItem>();
        }
    }
}