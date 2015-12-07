using AutoMapper;
using StructureMap.Configuration.DSL;

namespace SupportManager.Web.Infrastructure.Mapping
{
    public class AutoMapperRegistry : Registry
    {
        public AutoMapperRegistry()
        {
            Scan(scan =>
            {
                scan.AssemblyContainingType<AutoMapperRegistry>();
                scan.AddAllTypesOf<Profile>();
                scan.WithDefaultConventions();
            });
        }
    }
}