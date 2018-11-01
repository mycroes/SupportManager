using AutoMapper;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.DAL;

namespace SupportManager.Web.Api.Shared
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SupportTeam, TeamDto>();
            CreateMap<UserEmailAddress, EmailAddress>();
            CreateMap<UserPhoneNumber, PhoneNumber>();
            CreateMap<DAL.User, UserDetails>();
            CreateMap<ScheduledForward, ForwardRegistration>().ForMember(dst => dst.UserName,
                opt => opt.MapFrom(src => src.PhoneNumber.User.DisplayName));
            CreateMap<ForwardingState, ForwardRegistration>().ForMember(dst => dst.UserName,
                opt => opt.MapFrom(src => src.DetectedPhoneNumber.User.DisplayName));
        }
    }
}
