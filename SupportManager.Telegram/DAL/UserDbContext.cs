using Microsoft.EntityFrameworkCore;

namespace SupportManager.Telegram.DAL
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }
    }
}
