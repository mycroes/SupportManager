using AutoMapper;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserExistsQuery, UserExistsResponse>(MemberList.Source);
            CreateMap<DAL.User, UserExistsResponse>()
                .ForMember(dest => dest.IsExistingUser, opt => opt.UseValue(true));
            CreateMap<CreateCommand, DAL.User>(MemberList.Source);
            CreateMap<DAL.User, DetailsModel>();
            CreateMap<UserPhoneNumber, PhoneListModel>();
            CreateMap<EmailAddressCreateModel, UserEmailAddress>(MemberList.Source);
            CreateMap<UserEmailAddress, EmailAddressListModel>();
            CreateMap<PhoneNumberCreateModel, UserPhoneNumber>(MemberList.Source);
        }
    }
}