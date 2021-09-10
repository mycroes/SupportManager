using Microsoft.EntityFrameworkCore;
using SupportManager.Telegram.DAL;

namespace SupportManager.Telegram.Infrastructure
{
    internal static class DbContextFactory
    {
        public static UserDbContext Create(string filename)
        {
            var builder = new DbContextOptionsBuilder<UserDbContext>();
            builder.UseSqlite($"Data Source={filename}");

            return new UserDbContext(builder.Options);
        }
    }
}
