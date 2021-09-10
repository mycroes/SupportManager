using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportManager.Telegram.DAL;

namespace SupportManager.Telegram
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<UserDbContext>(ConfigureDbContext);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }

        private void ConfigureDbContext(IServiceProvider services, DbContextOptionsBuilder builder)
        {
            var configuration = services.GetService<Configuration>();
            builder.UseSqlite($"Data Source={configuration.DbFileName}");
        }
    }
}