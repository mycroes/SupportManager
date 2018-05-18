using AutoMapper;

namespace SupportManager.Web.Areas.Admin.User
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DAL.User, Index.Result.User>();
        }
    }
}