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
            CreateMap<SupportTeam, SupportManager.Api.Teams.Team>();
            CreateMap<UserEmailAddress, EmailAddress>();
            CreateMap<UserPhoneNumber, PhoneNumber>();
            CreateMap<DAL.User, UserDetails>();
            CreateMap<DAL.User, SupportManager.Api.Users.User>();
            CreateMap<ScheduledForward, ForwardRegistration>().ForMember(dst => dst.User,
                opt => opt.MapFrom(src => src.PhoneNumber.User));
            CreateMap<ForwardingState, ForwardRegistration>().ForMember(dst => dst.User,
                opt => opt.MapFrom(src => src.DetectedPhoneNumber.User));
        }
    }
}
