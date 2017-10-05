using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using HtmlTags;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.Tags;

namespace SupportManager.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt =>
                {
                    opt.Filters.Add<DbContextTransactionFilter>();
                    opt.Filters.Add<ValidatorActionFilter>();
                    opt.ModelBinderProviders.Insert(0, new EntityModelBinderProvider());
                })
                .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddScoped(_ => new SupportManagerContext(Configuration["Connections:SupportManager"]));
            services.AddHtmlTags(new DefaultAspNetMvcHtmlConventions());
            services.AddHangfire(hangfire => hangfire.UseSqlServerStorage(Configuration["Connections:hangfire"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseMvc(routes =>
            {
                routes.MapRoute("Team", "Admin/Team/{teamId:int}/{action}", new {area = "Admin", controller = "Team"});

                routes.MapRoute(name: "areaRoute", template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}