using System.Data.Entity.ModelConfiguration;

namespace SupportManager.DAL.Configuration
{
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public UserConfiguration()
        {
            HasMany(u => u.PhoneNumbers).WithRequired(p => p.User);
            HasMany(u => u.EmailAddresses).WithRequired(e => e.User);
            HasMany(u => u.Memberships).WithRequired(m => m.User);
        }
    }
}
