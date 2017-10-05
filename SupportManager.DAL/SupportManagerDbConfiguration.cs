using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace SupportManager.DAL
{
    public class SupportManagerDbConfiguration : DbConfiguration
    {
        public SupportManagerDbConfiguration()
        {
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
        }
    }
}
