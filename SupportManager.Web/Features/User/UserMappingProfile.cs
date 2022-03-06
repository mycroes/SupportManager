using AutoMapper;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<CreateCommand, DAL.User>(MemberList.Source);
            CreateMap<EmailAddressCreateModel, UserEmailAddress>(MemberList.Source);
            CreateMap<PhoneNumberCreateModel, UserPhoneNumber>(MemberList.Source);
        }
    }
}