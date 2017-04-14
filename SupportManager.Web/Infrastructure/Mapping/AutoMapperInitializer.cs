using AutoMapper;

namespace SupportManager.Web.Infrastructure.Mapping
{
    public class AutoMapperInitializer
    {
        public static void Initialize()
        {
            Mapper.Initialize(cfg => cfg.AddProfiles(typeof(AutoMapperInitializer)));
        }
    }
}