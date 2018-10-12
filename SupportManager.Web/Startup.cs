using System;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Console;
using HtmlTags;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.Tags;

[assembly: AspMvcViewLocationFormat(@"~\Features\{1}\{0}.cshtml")]
[assembly: AspMvcViewLocationFormat(@"~\Features\{0}.cshtml")]
[assembly: AspMvcViewLocationFormat(@"~\Features\Shared\{0}.cshtml")]

[assembly: AspMvcAreaViewLocationFormat(@"~\Areas\{2}\{1}\{0}.cshtml")]
[assembly: AspMvcAreaViewLocationFormat(@"~\Areas\{2}\{0}.cshtml")]
[assembly: AspMvcAreaViewLocationFormat(@"~\Areas\{2}\Shared\{0}.cshtml")]

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
                    opt.ModelBinderProviders.Insert(0, new LoggedInUserModelBinderProvider());
                })
                .AddFeatureFolders()
                .AddAreaFeatureFolders()
                .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddScoped(_ => new SupportManagerContext(Configuration["Connections:SupportManager"]));
            services.AddHtmlTags(new SupportManagerHtmlConventions());
            services.AddHangfire(hangfire =>
            {
                hangfire.UseSqlServerStorage(Configuration["Connections:hangfire"]);
                hangfire.UseConsole();
            });
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

            app.UseHangfireDashboard(
                options: new DashboardOptions {Authorization = new[] {new HangfireAuthorizationFilter()}});

            app.UseMvc(routes =>
            {
                routes.MapRoute("Teams", "Teams/{teamId:int}/{controller=Home}/{action=Index}", new {area = "Teams"});

                routes.MapRoute(name: "areaRoute", template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}